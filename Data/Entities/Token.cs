namespace WebApplication1.Data.Entities
{
    public class Token
    {
        public Guid      Id         { get; set; }
        public String    UserId     { get; set; }
        public DateTime  ExpiresAt  { get; set; }
    }
}
