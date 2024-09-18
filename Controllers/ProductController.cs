using WebApplication1.Data;
using WebApplication1.Models.Group;
using WebApplication1.Models.Shop;
using WebApplication1.Services.Upload;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController(IFileUploader fileUploader, DataContext dataContext) : ControllerBase
    {
        private readonly IFileUploader _fileUploader = fileUploader;
        private readonly DataContext _dataContext = dataContext;

        [HttpPost]

        public async Task<object> DoPost(ShopProductFormModel formModel)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole != "Admin")
            {
                return new { code = 403, status = "Forbidden", message = "You do not have permission to add products." };
            }
            String uploadedName;
            try
            {
                uploadedName = _fileUploader.UploadFile(
                    formModel.ImageFile,
                    "./Uploads/Shop"
                );
                _dataContext.Products.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Name = formModel.Name,
                    Description = formModel.Description,
                    Image = uploadedName,
                    DeleteDt = null,
                    Slug = formModel.Slug,
                    Price = formModel.Price,
                    Amount = formModel.Amount,
                    GroupId = formModel.GroupId,
                });
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new { code = 500, status = "error", message = ex.Message };
            }

            return new { code = 200, status = "OK", message = "Created" };
        }
    }
}