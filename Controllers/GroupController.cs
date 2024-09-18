﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models.Group;
using WebApplication1.Services.Upload;

namespace WebApplication1.Controllers
{
    [Route("api/group")]
    [ApiController]
    public class GroupController(IFileUploader fileUploader, DataContext dataContext) : ControllerBase
    {
        private readonly IFileUploader _fileUploader = fileUploader;
        private readonly DataContext _dataContext = dataContext;

        [HttpPost]
        public async Task<object> DoPost(GroupFormModel formModel)
        {
            String uploadedName;
            try
            {
                uploadedName = _fileUploader.UploadFile(
                    formModel.ImageFile,
                    "./Uploads/Shop"
                );
                _dataContext.Groups.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Name = formModel.Name,
                    Description = formModel.Description,
                    Image = uploadedName,
                    DeleteDt = null,
                    Slug = formModel.Slug,
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