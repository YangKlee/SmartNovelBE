namespace SmartNovelBE.Models
{
    public class RateNovelRequest
    {
        public string NovelId { get; set; } = null!;

        public int RatingValue { get; set; }
    }
}
