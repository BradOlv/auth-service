using Microsoft.EntityFrameworkCore;
using AuthService.Domain;

namespace AuthService.Persistence;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Aquí puedes agregar configuraciones extra, como que el email sea único
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
    }
}
