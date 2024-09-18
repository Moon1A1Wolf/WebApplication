namespace WebApplication1.Services.Upload
{
    public interface IFileUploader
    {
        String UploadFile(IFormFile file, String? path);
    }
}
