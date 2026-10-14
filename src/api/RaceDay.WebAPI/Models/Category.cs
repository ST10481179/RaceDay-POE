namespace RaceDay.WebAPI.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public string? AgeRange { get; set; }

        public int EventID { get; set; }
        public Event? Event { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Race> Races { get; set; } = new List<Race>();
    }
}
