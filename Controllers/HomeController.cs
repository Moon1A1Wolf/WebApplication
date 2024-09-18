using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Models.Home;
using WebApplication1.Models.Shop;
using WebApplication1.Services.FileName;
using WebApplication1.Services.Kdf;
using WebApplication1.Services.OTP;
using WebApplication1.Servises.Hash;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        // приклад інжекції - _loger
        private readonly ILogger<HomeController> _logger;
        // інжектуємо нащ (хеш-) сервіс
        private readonly IHashService _hashService;
        private readonly DataContext _dataContext;
        private readonly IKdfService _kdfService;

        private String fileErrorKey = "file-error";
        private String fileNameKey = "file-name";

        public HomeController(ILogger<HomeController> logger, IHashService hashService, DataContext dataContext, IKdfService kdfService)
        {
            _logger = logger;
            _hashService = hashService;
            _dataContext = dataContext;
            _kdfService = kdfService;
            /* Інжекція через конструктор - найбільш рекомендований варіант.
             * Контроллер служб (інжектор) аналізує параметри конструктора і 
             * сам підставляє до нього необхідні об'єкти (інстанси) служб
             */
        }


            //private readonly IOTPGenerator _otpGenerator;

            //public HomeController(IOTPGenerator otpGenerator)
            //{
            //    _otpGenerator = otpGenerator;
            //}


            //private readonly IFileNameGenerator _fileNameGenerator;

            //public HomeController(IFileNameGenerator fileNameGenerator)
            //{
            //    _fileNameGenerator=fileNameGenerator;
            //}

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Shop()
        {
            return RedirectToAction("Index", "Shop");
        }

        public IActionResult Profile()
        {
            // if (HttpContext.User.Identity?.IsAuthenticated == true)
            if (HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                String sid = HttpContext
                    .User
                    .Claims
                    .First(c => c.Type == ClaimTypes.Sid)
                    .Value;

                return View(new ProfilePageModel()
                {
                    User = _dataContext
                        .Users
                        .Include(u => u.Feedbacks)
                            .ThenInclude(f => f.Product)
                        .First(u => u.Id.ToString() == sid),
                });
            }
            return RedirectToAction(nameof(this.Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Intro()
        {
            return View();
        }

	public IActionResult Razor()
	{
	     return View();
	}

        public IActionResult URL()
        {
            return View();
        }

        public IActionResult Ioc()
        {
            ViewData["hash"] = _hashService.Digest("123");
            ViewData["hashCode"] = _hashService.GetHashCode();
            return View();
        }

        //[HttpGet]
        //public IActionResult OTP()
        //{
        //    var otp = _otpGenerator.GenerateOtp();
        //    ViewBag.Otp = otp;
        //    return View();
        //}

        //public IActionResult FileName()
        //{
        //    var name = _fileNameGenerator.GenerateFileName(12);
        //    ViewBag.FileName = name;
        //    return View();
        //}


        [HttpGet]
        public IActionResult AddProduct()
        {
            Product product = new Product();

            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Product")))    
            {
                try
                {
                    string json = HttpContext.Session.GetString("Product");
                    product = JsonSerializer.Deserialize<Product>(json);
                }
                catch (Exception ex)
                {
                    ViewBag.Message = $"Помилка десеріалізації даних: {ex.Message}";
                }
            }

            if (product != null/* && !string.IsNullOrEmpty(product.Name)*/)
            {
                ViewBag.Message = "Останні передані дані:";
                ViewBag.Product5 = HttpContext.Session.GetString("Product");
            }
            else
            {
                ViewBag.Message = "Немає попередніх даних.";
            }

            return View();
        }

        [HttpPost]
        public IActionResult AddProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Будь ласка, виправте помилки у формі.";
                return View(product);
            }
            string fileName = null;

            if (product.ProductImg != null)
            {
                String ext = Path.GetExtension(product.ProductImg.FileName);

                if (string.IsNullOrEmpty(ext))
                {
                    HttpContext.Session.SetString(fileErrorKey, "Файли без розширення не приймаються");
                }
                else
                {
                    var allowedExtensions = new[] { ".jpg", ".png", ".bmp" };

                    if (allowedExtensions.Contains(ext.ToLower()))
                    {
                        String path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        do
                        {
                            fileName = Guid.NewGuid().ToString() + ext;
                        } while (System.IO.File.Exists(Path.Combine(path, fileName)));

                        var filePath = Path.Combine(path, fileName);
                        using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            product.ProductImg.CopyTo(fileStream);
                        }
                        HttpContext.Session.SetString(fileNameKey, fileName);
                    }
                    else
                    {
                        HttpContext.Session.SetString(fileErrorKey, "Файл має недозволене розширення");
                    }
                }
            }
            //TempData["Product"] = JsonSerializer.Serialize(product);
            var dictionary = new Dictionary<string, object>();
            foreach (PropertyInfo property in product.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.Name != "ProductImg")
                {
                    dictionary[property.Name] = property.GetValue(product, null);
                }
            }
            dictionary["imgPath"] = fileName;
            HttpContext.Session.SetString("Product", JsonSerializer.Serialize(dictionary));
            return RedirectToAction("AddProduct");
        }

        public IActionResult SignUp()
        {
            SignUpPageModel model = new();

            // На початку перевіряємо чи є збережена сесія
            if (HttpContext.Session.Keys.Contains("signup-data"))
            {
                // є дані це редирект, обробляємо дані
                var formModel = JsonSerializer.Deserialize<SignUpFormModel>(
                    HttpContext.Session.GetString("signup-data")!)!;

                model.FormModel = formModel;
                model.ValidationErrors = _Validate(formModel);

                if (model.ValidationErrors.Where(p => p.Value != null).Count() == 0)
                {
                    // íåìàº ïîìèëîê âàë³äàö³¿ - ðåºñòðóºìî ó ÁÄ
                    String salt = _hashService.Digest(Guid.NewGuid().ToString())[..20];
                    _dataContext.Users.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Name = formModel.UserName,
                        Email = formModel.UserEmail,
                        Salt = salt,
                        Dk = _kdfService.DerivedKey(formModel.UserPassword, salt),
                        Registered = DateTime.Now,
                        Avatar = HttpContext.Session.GetString(fileNameKey)
                    });
                    _dataContext.SaveChanges();
                }

                ViewData["data"] = $"email: {formModel.UserEmail}, name: {formModel.UserName}";

                // Ім'я завантаженого файлу (аватарки) також зберігається у сесії
                if (HttpContext.Session.Keys.Contains(fileNameKey))
                {
                    ViewData["avatar"] = HttpContext.Session.GetString(fileNameKey);
                    HttpContext.Session.Remove(fileNameKey);
                }

                // Видаляємо дані з сесії, щоб уникнути повторного оброблення
                HttpContext.Session.Remove("signup-data");
            }
            return View(model);
        }

        public IActionResult Demo([FromQuery(Name="user-email")] String userEmail, [FromQuery(Name = "user-name")] String userName)
        {
            /* Прийом даних від форми, варіант 1: через параметри action 
             * Зв'язування автоматично відбувається за збігом імен
             * <input name="userName"/> ----- Demo(String userName)
             * якщо в HTML використовуються імена, які неможливі у С#
             * (user-name), TO додається атрибут [From...] 13 зазначенням імені
             * перед потрібним параметром
             * 
             * Варіант 1 використовується коли кількість параметрів невелика (1-2)
             * Більш рекомендований спосіб використання моделей
             */
            ViewData["data"] = $"email: {userEmail}, name: {userName}";
            return View();
        }

        public IActionResult RegUser(SignUpFormModel formModel)
        {
            HttpContext.Session.SetString("signup-data", JsonSerializer.Serialize(formModel));
            
            if (formModel.UserAvatar != null) 
            {
                // 1. Відокремити розширення файлу
                int dotPosition = formModel.UserAvatar.FileName.IndexOf(".");
                if(dotPosition == -1) // немає розширення файл
                {
                    HttpContext.Session.SetString(fileErrorKey, "Файли без розширення не приймаються");
                }
                else
                {
                    String ext = formModel.UserAvatar.FileName[dotPosition..];
                    // 2.Перевірити розширення на перелік дозволених
                    if (ext == ".jpg" || ext == ".png" || ext == ".bmp")
                    {
                        // 3.Сформувати ім'я файлу, переконатись, що не перекривається наявний файл
                        String fileName;
                        String path = "./Uploads/"; //"./wwwroot/img/upload/";
                        do
                        {
                            fileName = Guid.NewGuid().ToString() + ext;
                        } while (System.IO.File.Exists(path + fileName));

                        // 4.Зберегти файл, зберегти у БД ім'я файлу.
                        using Stream writer = new StreamWriter(path + fileName).BaseStream;
                        formModel.UserAvatar.CopyTo(writer);
                        HttpContext.Session.SetString(fileNameKey, fileName);
                    }
                    else
                    {
                        HttpContext.Session.SetString(fileErrorKey, "Файл не має розширення");
                    }
                }
            }
            return RedirectToAction(nameof(SignUp));

            //ViewData["data"] = $"email: {formModel.UserEmail}, name: {formModel.UserName}";
            //return View("Demo");
            /* Проблема: якщо сторінка побудована через передачу форми, тο її оновлення у браузері
             *   а) видає повідомлення, на яке ми не впливаємо
             *   6) повторно передає дані форми, що може призвести до дублювання даних у БД, файлів, тощо
             * Рішення: "скидання даних" - переадресація відповіді із запам'ятовуванням даних
             *
             * Client(Browser)                      Server (ASP)
             * [form]-------- POST RegUser -----------> [form]---Session
             * <-------------- 301 SignUp -------------             |
             * --------------- GET SignUp ------------>             |
             * <------------------ HTML ----------------------   оброблення
             * 
             * Включення та налаштування сесій див. https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state
             */
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Download ([FromRoute] String id)
        {
            // id закладена в маршрутизаторі назва, суть - ім'я файлу
            id = id.Replace('_', '/');
            String filename = $"./Uploads/{id}";
            if (System.IO.File.Exists(filename))
            {
                var stream = new StreamReader(filename).BaseStream;
                return File(stream, "image/png");
            }
            return NotFound();
        }

        private Dictionary<string, string?> _Validate(SignUpFormModel model)
        {
            /* Валідація перевірка даних на відповідність певним шаблонам/правилам
             * Результат валідації - 
             * {
             *    "UserEmail": null,              null - як ознака успішної валідації 
             *    "UserName": "Too short"         значення повідомлення про помилку
             * }
             */
            Dictionary<string, string?> res = new();

            var emailRegex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
            res[nameof(model.UserEmail)] = 
                String.IsNullOrEmpty(model.UserEmail)
                ? "Не допускається порожнє поле"
                : emailRegex.IsMatch(model.UserEmail)
                    ? null
                    : "Введіть коректну адресу";

            var nameRegex = new Regex(@"^\w{2,}(\s+\w{2,})*$");
            res[nameof(model.UserName)] =
                String.IsNullOrEmpty(model.UserName)
                ? "Не допускається порожнє поле"
                : nameRegex.IsMatch(model.UserName)
                    ? null
                    : "Введіть коректне ім'я";

            if(String.IsNullOrEmpty(model.UserPassword))
            {
                res[nameof(model.UserPassword)] = "Не допускається порожнє поле";
            }
            else if(model.UserPassword.Length < 8)
            {
                res[nameof(model.UserPassword)] = "Пароль не має бути коротшим за 8 символів";
            }
            else 
            {
                List<string> parts = [];
                if (!Regex.IsMatch(model.UserPassword, @"\d"))
                {
                    parts.Add(" одну цифру");
                }
                if (!Regex.IsMatch(model.UserPassword, @"\D"))
                {
                    parts.Add(" одну літеру");
                }
                if (!Regex.IsMatch(model.UserPassword, @"\W"))
                {
                    parts.Add(" однин спецсимвол");
                }
                if (parts.Count > 0)
                {
                    res[nameof(model.UserPassword)] =
                        "Пароль повинен містити щонайменше" + String.Join(',', parts);
                }
                else
                {
                    res[nameof(model.UserPassword)] = null;
                }
            }

            res[nameof(model.UserRepeat)] = model.UserPassword == model.UserRepeat
                ? null
                : "Паролі не збігаються";

            res[nameof(model.IsAgree)] = model.IsAgree
                ? null
                : "Необхідно прийняти правила сайту";

            //Результати перевірки файлу збережені у сесії
            if (HttpContext.Session.Keys.Contains(fileErrorKey)) 
            {
                res[nameof(model.UserAvatar)] = HttpContext.Session.GetString(fileErrorKey);
                HttpContext.Session.Remove(fileErrorKey);
            }

            return res;
        }
    }
}
