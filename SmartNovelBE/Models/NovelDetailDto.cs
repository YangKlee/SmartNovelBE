namespace SmartNovelBE.DTOs.Novel
{
    public class NovelDetailDto
    {
        public string NovelId { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public string? Description { get; set; }

        public string AgeRating { get; set; } = null!;

        public string? ImageNovelUrl { get; set; }

        public string? ImageBanerNovelUrl { get; set; }

        public string Status { get; set; } = null!;

        public string AuthorId { get; set; } = null!;

        public string AuthorName { get; set; } = null!;

        public int ViewCount { get; set; }

        public int LikeCount { get; set; }

        public DateTime? CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        public int TotalChapters { get; set; }

        public List<string> Categories { get; set; } = new();
    }
}