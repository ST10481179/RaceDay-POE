using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>()
                .Property(u => u.Gender)
                .HasConversion<string>()
                .HasMaxLength(20);

        }
    }
}
