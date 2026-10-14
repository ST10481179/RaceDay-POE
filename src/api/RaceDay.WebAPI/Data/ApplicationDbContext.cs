using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RaceDay.WebAPI.Models;

namespace RaceDay.WebAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Participant> Participants { get; set; }
        public DbSet<Sponsor> Sponsors { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Race> Races { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.Property(value => value.LoginProvider).HasMaxLength(128);
                entity.Property(value => value.ProviderKey).HasMaxLength(128);
            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.Property(value => value.LoginProvider).HasMaxLength(128);
                entity.Property(value => value.Name).HasMaxLength(128);
            });

            builder.Entity<Participant>(entity =>
            {
                entity.ToTable("Participant");
                entity.Property(value => value.FirstName).HasMaxLength(100).IsRequired();
                entity.Property(value => value.LastName).HasMaxLength(100).IsRequired();
                entity.Property(value => value.Email).HasMaxLength(100).IsRequired();
                entity.HasIndex(value => value.Email).IsUnique();
            });

            builder.Entity<Sponsor>(entity =>
            {
                entity.ToTable("Sponsor");
                entity.Property(value => value.SponsorName).HasMaxLength(100).IsRequired();
                entity.Property(value => value.ContactInfo).HasMaxLength(150);
            });

            builder.Entity<Event>(entity =>
            {
                entity.ToTable("Event");
                entity.Property(value => value.EventName).HasMaxLength(100).IsRequired();
                entity.Property(value => value.EventDate).HasColumnType("date");
                entity.Property(value => value.Location).HasMaxLength(150).IsRequired();
                entity.HasOne(value => value.Sponsor)
                    .WithMany(value => value.Events)
                    .HasForeignKey(value => value.SponsorID)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");
                entity.Property(value => value.CategoryName).HasMaxLength(100).IsRequired();
                entity.Property(value => value.Distance).HasColumnType("decimal(5,2)");
                entity.Property(value => value.AgeRange).HasMaxLength(50);
                entity.HasOne(value => value.Event)
                    .WithMany(value => value.Categories)
                    .HasForeignKey(value => value.EventID)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<Enrollment>(entity =>
            {
                entity.ToTable("Enrollment");
                entity.Property(value => value.Status).HasMaxLength(50).HasDefaultValue("Registered");
            });

            builder.Entity<Race>(entity =>
            {
                entity.ToTable("Race");
                entity.Property(value => value.Distance).HasColumnType("decimal(5,2)");
            });

            builder.Entity<Result>(entity =>
            {
                entity.ToTable("Result");
                entity.Property(value => value.FinishTime).HasColumnType("time");
            });

            builder.Entity<Enrollment>()
                .HasOne(e => e.Participant)
                .WithMany(p => p.Enrollments)
                .HasForeignKey(e => e.ParticipantID)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Enrollment>()
                .HasOne(e => e.Event)
                .WithMany(ev => ev.Enrollments)
                .HasForeignKey(e => e.EventID)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Enrollment>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.NoAction);

            // Result relationship
            builder.Entity<Result>()
                .HasOne(r => r.Enrollment)
                .WithMany(e => e.Results)
                .HasForeignKey(r => r.EnrollmentID)
                .OnDelete(DeleteBehavior.NoAction);

            // Race relationship
            builder.Entity<Race>()
                .HasOne(r => r.Category)
                .WithMany(c => c.Races)
                .HasForeignKey(r => r.CategoryID)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
