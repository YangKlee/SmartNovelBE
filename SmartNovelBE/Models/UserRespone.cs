namespace SmartNovelBE.Models
{
    public class UserRespone
    {
        // dđồng bộ vs angular
        public record NovelResponseAuthor
        {
            public string NovelId { get; init; } = string.Empty;
            public string Title { get; init; } = string.Empty;
            public string? Slug { get; init; }
            public string? Description { get; init; }
            public string? AgeRating { get; init; }
            public string? ImageNovelUrl { get; init; }
            public string? ImageBanerNovelUrl { get; init; }
            public string? Status { get; init; }
            public string? Uid { get; init; }

            public int ViewCount { get; init; }
            public int LikeCount { get; init; }

            public int CountChapter { get; init; }
            public int CountChapterPublic { get; init; }
            public int CountChapterDraft { get; init; }
            public int CountChapterRemove { get; init; }

            public double NovelRating { get; init; }
            public int NovelCountComment { get; init; }

            public DateTime CreateTime { get; init; }
            public DateTime UpdateTime { get; init; }
        }
    }
}