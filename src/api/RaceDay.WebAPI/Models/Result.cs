namespace RaceDay.WebAPI.Models
{
    public class Result
    {
        public int ResultID { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }

        public int EnrollmentID { get; set; }
        public Enrollment Enrollment { get; set; }
    }
}
