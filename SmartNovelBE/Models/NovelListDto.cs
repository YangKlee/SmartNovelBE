namespace SmartNovelBE.DTOs.Novel
{
    public class NovelListDto
    {
        public string NovelId { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string? ImageNovelUrl { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public string Status { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}

