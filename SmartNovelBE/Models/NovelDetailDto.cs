namespace SmartNovelBE.DTOs.Novel
{
    public class NovelDetailDto
    {
        public string NovelId { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public string? Description { get; set; }

        public string? ImageNovelUrl { get; set; }

        public string? ImageBanerNovelUrl { get; set; }

        public string AuthorName { get; set; }

        public int ViewCount { get; set; }

        public int LikeCount { get; set; }

        public List<string> Categories { get; set; }
    }
}
