using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Data
{
    public class DataContext : DbContext
    {
       

        public DbSet<Entities.User>  Users  { get; set; }
        public DbSet<Entities.Token> Tokens { get; set; }

        public DataContext(DbContextOptions options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entities.User>()
                .HasIndex(u => u.Email)
                .IsUnique();
           // modelBuilder.Entity<Entities.Token>()
           //     .HasOne(t => t.UserId)
            //    .WithMany();


        }
    }
}
