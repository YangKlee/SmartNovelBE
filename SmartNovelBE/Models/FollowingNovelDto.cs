namespace SmartNovelBE.Models
{
    public class FollowingNovelDto
    {
        public string NovelId { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string? Slug { get; set; }

        public string? ImageNovelUrl { get; set; }

        public string AuthorName { get; set; } = null!;

        public string? Status { get; set; }

        public int ViewCount { get; set; }

        public int LikeCount { get; set; }

        public double AverageRating { get; set; }

        public int TotalChapter { get; set; }
    }
}
