using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.Data
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Keep explicit constructor for clarity and compatibility")]
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<RaceDay.Models.Event>? Events { get; set; }
        public DbSet<RaceDay.Models.Category>? Categories { get; set; }
        public DbSet<RaceDay.Models.Enrolment>? Enrolments { get; set; }
        public DbSet<RaceDay.Models.Result>? Results { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>()
                .Property(u => u.Gender)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Prevent multiple cascade paths in SQL Server by restricting delete on relationships
            // where a user can be related to entities that in turn relate to other entities (e.g. Organiser -> Event -> Result
            // and Participant -> Result). Configure the delete behavior explicitly to NoAction/Restrict.
            builder.Entity<RaceDay.Models.Result>()
                .HasOne(r => r.Participant)
                .WithMany()
                .HasForeignKey(r => r.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RaceDay.Models.Enrolment>()
                .HasOne(e => e.Participant)
                .WithMany()
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RaceDay.Models.Event>()
                .HasOne(ev => ev.Organiser)
                .WithMany()
                .HasForeignKey(ev => ev.OrganiserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent cascade delete paths between Events -> Categories/Enrolments/Results
            // which can cause SQL Server "multiple cascade paths" errors. Configure
            // these relationships to Restrict (NO ACTION) so deletions must be explicit.
            builder.Entity<RaceDay.Models.Category>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RaceDay.Models.Enrolment>()
                .HasOne(en => en.Event)
                .WithMany(e => e.Enrolments)
                .HasForeignKey(en => en.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RaceDay.Models.Result>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Results)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
