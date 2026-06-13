namespace SmartNovelBE.Controllers
{
    public class RateNovelResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public double AverageRating { get; set; }

        public int TotalRatings { get; set; }
    }
}
