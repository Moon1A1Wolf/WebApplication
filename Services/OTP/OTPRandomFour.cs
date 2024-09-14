namespace WebApplication1.Services.OTP
{
    public class OTPRandomFour : IOTPGenerator
    {
        private static Random random = new Random();
        public string GenerateOtp()
        {
            return random.Next(0, 10000).ToString("D4");
        }
    }
}
