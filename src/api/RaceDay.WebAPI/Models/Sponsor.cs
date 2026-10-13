namespace RaceDay.WebAPI.Models
{
    public class Sponsor
    {
        public int SponsorID { get; set; }
        public string SponsorName { get; set; } = string.Empty;
        public string? ContactInfo { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
