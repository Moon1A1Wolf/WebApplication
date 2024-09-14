namespace WebApplication1.Services.OTP
{
    public class OTPRandomSix : IOTPGenerator
    {
        private static Random random = new Random();
        public string GenerateOtp()
        {
            return random.Next(0, 1000000).ToString("D6");
        }
    }
}
