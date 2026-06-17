namespace SmartNovelBE.DTOs.Novel
{
    public class RateNovelRequest
    {
        public string NovelId { get; set; } = null!;
        public int RatingValue { get; set; }
    }
}
