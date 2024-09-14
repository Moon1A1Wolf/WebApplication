using System.Text;

namespace WebApplication1.Services.FileName
{
    public class FileNameGenerator : IFileNameGenerator
    {
        private static readonly char[] AllowedChars = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
        private static readonly Random _random = new Random();

        public string GenerateFileName(int length)
        {
            var fileName = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                fileName.Append(AllowedChars[_random.Next(AllowedChars.Length)]);
            }

            return fileName.ToString();
        }
    }
}
