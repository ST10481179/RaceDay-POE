using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Enrolment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string ParticipantId { get; set; } = string.Empty;

        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [ForeignKey("ParticipantId")]
        public RaceDay.Data.ApplicationUser? Participant { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    }
}