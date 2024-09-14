namespace WebApplication1.Services.Kdf
{
    public interface IKdfService
    {
        String DerivedKey(String password, String salt);
    }
}
