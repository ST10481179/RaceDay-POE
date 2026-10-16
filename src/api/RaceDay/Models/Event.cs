using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RaceDay.Data;

namespace RaceDay.Models
{
    public enum EventType
    {
        Run,
        Walk,
        Cycle
    }

    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public double DistanceKm { get; set; }

        [Required]
        public EventType Type { get; set; }

        [Required]
        public string OrganiserId { get; set; } = string.Empty;

        [ForeignKey("OrganiserId")]
        public ApplicationUser? Organiser { get; set; }

        public ICollection<Category>? Categories { get; set; }

        public ICollection<Enrolment>? Enrolments { get; set; }

        public ICollection<Result>? Results { get; set; }
    }
}