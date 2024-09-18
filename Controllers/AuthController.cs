﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebApplication1.Data.Entities;
using WebApplication1.Data;
using WebApplication1.Services.Kdf;

namespace WebApplication1.Controllers
{

    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IKdfService _kdfService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(DataContext dataContext, IKdfService kdfService, ILogger<AuthController> logger)
        {
            _dataContext = dataContext;
            _kdfService = kdfService;
            _logger = logger;
        }

        [HttpGet]
        public object DoGet(String email, String password)
        {
            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(password))
            {
                return new
                {
                    status = "Error",
                    code = 400,
                    message = "Email and password must not be empty"
                };
            }
            // Розшифрувати DK неможливо, тому повторюємо розрахунок DK з сіллю, що
            // зберігається у користувача, та паролем, який передано у параметрі
            bool isEmail;
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                isEmail = false;
            }
            else 
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(trimmedEmail);
                    isEmail = true;
                }
                catch
                {
                    isEmail = false;
                }
            }

            User user;
            DateTime birthDate;
            if (isEmail)
            {
                user = _dataContext
                    .Users
                    .FirstOrDefault(u =>
                        u.Email == trimmedEmail &&
                        u.DeleteDt == null);  // додаємо умову, що користувач не видалений
            }
            else if (DateTime.TryParse(email, out birthDate))
            {
                user = _dataContext
                    .Users
                    .FirstOrDefault(u =>
                        u.Birthdate == birthDate &&
                        u.DeleteDt == null);  // додаємо умову, що користувач не видалений
            }
            else
            {
                user = _dataContext
                    .Users
                    .FirstOrDefault(u =>
                        u.Name == email &&
                        u.DeleteDt == null);  // додаємо умову, що користувач не видалений

            }

            if (user != null && _kdfService.DerivedKey(password, user.Salt) == user.Dk)
            {
                Token token = _dataContext.Tokens.FirstOrDefault(t => t.UserId == user.Id);

                if (token == null)
                {
                    token = new Token
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        ExpiresAt = DateTime.Now.AddHours(3),
                    };
                    _dataContext.Tokens.Add(token);
                    _dataContext.SaveChanges();
                }

                HttpContext.Session.SetString("token", token.Id.ToString());
                return new
                {
                    status = "Ok",
                    code = 200,
                    message = token.Id
                };
            }
            else
            {
                return new
                {
                    status = "Reject",
                    code = 401,
                    message = "Credentials rejected"
                };
            }
        }

        [HttpDelete]
        public object DoDelete()
        {
            HttpContext.Session.Remove("token");
            return "Ok";
        }

        [HttpPut]
        public async Task<object> DoPutAsync()
        {
            // Дані, що передаються в тілі запиту доступні через Request.Body
            String body = await new StreamReader(Request.Body).ReadToEndAsync();

            _logger.LogWarning(body);

            JsonNode json = JsonSerializer.Deserialize<JsonNode>(body)
                ?? throw new Exception("JSON in body is invalid");

            String? email = json["email"]?.GetValue<String>();
            String? name = json["name"]?.GetValue<String>();
            String? birthdate = json["birthdate"]?.GetValue<String>();

            if (email == null && name == null && birthdate == null)
            {
                return new { code = 400, status = "Error", message = "No data" };
            }
            if (email != null)
            {
                var emailRegex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
                if (!emailRegex.IsMatch(email))
                {
                    return new { code = 422, status = "Error", message = "Email match no pattern" };
                }
            }
            DateTime? birthDateTime = null;
            if (birthdate != null)
            {
                try
                {
                    birthDateTime = DateTime.Parse(birthdate);
                }
                catch
                {
                    return new { code = 422, status = "Error", message = "Birthdate unparseable" };
                }
            }

            Guid userId = Guid.Parse(
                HttpContext
                .User
                .Claims
                .First(c => c.Type == ClaimTypes.Sid)
                .Value);

            var user = _dataContext.Users.Find(userId);
            if (user == null)
            {
                return new { code = 403, status = "Error", message = "Forbidden" };
            }

            if (email != null)
            {
                user.Email = email;
            }
            if (name != null)
            {
                user.Name = name;
            }
            if (birthDateTime != null)
            {
                user.Birthdate = birthDateTime;
            }

            await _dataContext.SaveChangesAsync();

            return new { code = 200, status = "OK", message = "Updated" };
        }

        public async Task<object> DoOther()
        {
            switch (Request.Method)
            {
                case "UNLINK": return await DoUnlink();
                default:
                    return new
                    {
                        status = "error",
                        code = 405,
                        message = "Method Not Allowed"
                    };
            }
        }

        private async Task<object> DoUnlink()
        {
            Guid userId;
            try
            {
                userId = Guid.Parse(
                    HttpContext
                    .User
                    .Claims
                    .First(c => c.Type == ClaimTypes.Sid)
                    .Value
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("DoUnlink Exception: {ex}", ex.Message);
                return new
                {
                    code = 401,
                    status = "Error",
                    message = "UnAuthorized"
                };
            }

            var user = await _dataContext.Users.FindAsync(userId);
            if (user == null)
            {
                return new { code = 403, status = "Error", message = "Forbidden" };
            }

            user.DeleteDt = DateTime.Now;
            // Право бути забутим - видалення персональних даних
            user.Name = "";
            user.Email = "";
            user.Birthdate = null;
            if (user.Avatar != null)  // треба видалити файл-аватарку
            {
                String path = "./Uploads/";
                System.IO.File.Delete(path + user.Avatar);
                user.Avatar = null;
            }
            await _dataContext.SaveChangesAsync();   // фіксуємо зміни у БД
            this.DoDelete();   // видаляємо токен
            return new
            {
                status = "OK",
                code = 200,
                message = "Deleted"
            };
        }
    }
}
/*
 * Контролери розрізняють: MVC та API
 * MVC - різні адреси ведуть на різні дії (actions)
 *    /Home/Index -> Index()
 *    /Home/Db    -> Db()
 *    
 * API - різні методи запиту ведуть на різні дії
 *   GET  /api/auth  -> DoGet()
 *   POST /api/auth  -> DoPost()
 *   PUT  /api/auth  -> DoPut()
 *   
 *   
 * Токени авторизації  
 * Токен - "жетон", "перепустка" - дані, що видаються як результат
 * автентифікації і далі використовуються для "підтвердження особи" -
 * авторизації.
 *   
 *   
 */
/* CRUD: Delete
 * Особливості видалення даних
 * ! видалення створює проблеми за наявності зв'язків між даними
 * - замість видалення вводиться мітка "видалено" (у вигляді дати-часу видалення)
 * ! Art. 17 GDPR "Право бути забутим" - необхідність видалення персональних
 *   даних на вимогу користувача
 * - Класифікувати дані на персональні / не персональні, одні - видаляти, інші
 *   залишати.
 *   
 * = розглядається два варіанти видалень
 *  soft-delete - помітка видалення і у випадку людини стирання персональних даних
 *  hard-delete - повне видалення - допускається лише за відсутності зв'язків
 */