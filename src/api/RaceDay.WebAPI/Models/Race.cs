namespace RaceDay.WebAPI.Models
{
    public class Race
    {
        public int RaceID { get; set; }
        public decimal Distance { get; set; }

        public int CategoryID { get; set; }
        public Category? Category { get; set; }
    }
}
