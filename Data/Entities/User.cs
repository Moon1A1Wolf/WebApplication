namespace WebApplication1.Data.Entities
{
    public class User
    {
        public Guid      Id         { get; set; }
        public String    Email      { get; set; } = null!;
        public string    Name       { get; set; } = null!;
        public DateTime? Birthdate  { get; set; }
        public DateTime  Registered { get; set; }
    }
}
