namespace RaceDay.WebAPI.Models
{
    public class Event
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;

        public int? SponsorID { get; set; }
        public Sponsor? Sponsor { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
