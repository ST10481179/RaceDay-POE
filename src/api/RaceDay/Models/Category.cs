using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // e.g. Under 20, 10km

        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }

        public double? MinDistanceKm { get; set; }
        public double? MaxDistanceKm { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        public ICollection<Enrolment>? Enrolments { get; set; }
    }
}