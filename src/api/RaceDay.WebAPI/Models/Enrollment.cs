namespace RaceDay.WebAPI.Models
{
    public class Enrollment
    {
        public int EnrollmentID { get; set; }
        public int ParticipantID { get; set; }
        public Participant? Participant { get; set; }

        public int CategoryID { get; set; }
        public Category? Category { get; set; }

        public int EventID { get; set; }
        public Event? Event { get; set; }

        public string Status { get; set; } = "Registered";

        public ICollection<Result> Results { get; set; } = new List<Result>();
    }
}
