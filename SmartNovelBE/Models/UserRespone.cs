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
        public class NovelResponseAuthor2
        {
            public string NovelId { get; set; }
            public string Title { get; set; }
            public string Slug { get; set; }
            public string Description { get; set; }
            public string AgeRating { get; set; }
            public string ImageNovelUrl { get; set; }
            public string ImageBanerNovelUrl { get; set; }
            public string Status { get; set; }

            public int? ViewCount { get; set; }
            public int? LikeCount { get; set; }

            public DateTime? CreateTime { get; set; }
            public DateTime? UpdateTime { get; set; }
            public ICollection<Category> categories { get; set; }

            public int countChapter { get; set; }

            public int countChapterPublic { get; set; }
            public int countChapterDraf { get; set; }

            public int countChapterRemove { get; set; }

            public double novelRating { get; set; }
        }

        public record CommentResponse
        {
            public string? CommentId { get; init; }
            public string? NovelId { get; init; }
            public string? ChapterId { get; init; }
            public string? ParentCommentId { get; init; }
            public string? UserId { get; init; }
            public string? Content { get; init; }
            public string? DisplayName { get; init; }
            public string? UserAvatarUrl { get; init; }
            public DateTime? CommentDateTime { get; init; }
            public string? CurrentUserId { get; init; }
            public string? RoleId { get; init; }
            public bool IsAdminMode { get; init; } = false;
            public int CountChildComment { get; init; } = 0;
        }
        public record CommentReponeReal
        {
            public List<CommentResponse> comments { set; get; } = new();
            public int totalComment { set; get; } = 0;
        }
        public record DashboardAuthorStatsNovelViewModel
        {
            public int? TotalNovels { get; set; }
            public int? PublicNovels { get; set; }
            public int? RemovedNovels { get; set; }
            public int? DraftNovels { get; set; }

            public int? TotalChapters { get; set; }
            public int? PublicChapters { get; set; }
            public int? RemovedChapters { get; set; }
            public int? DraftChapters { get; set; }

        }
        public record DashboardAuthorStatsProflileViewModel
        {
            public int? countFollower { set; get; }
            public int? totalView { set; get; }
            //public int? creatorPoint { set; get; }

        }

        public record DashboardUserStatsInfo
        {
            public int TotalUsers { get; set; }
            public int AdminCount { get; set; }
            public int ModeratorCount { get; set; }
            public int AuthorCount { get; set; }
            public int ReaderCount { get; set; }
            public int ActiveCount { get; set; }
            public int BlockedCount { get; set; }
        }

        public record DashboardActivityStatsInfo
        {
            public string Date { get; set; } = string.Empty;
            public int ChaptersAdded { get; set; }
            public int NovelsAdded { get; set; }
        }
        public record NovelDetail
        {
            public Novel novel { set; get; }
            public string? firstChapter { set; get; }
            public string? newestChapter { set; get; }
            public string? readingChapter { set; get; }
            public bool isFollowNovel { set; get; } = false;
            public bool isFollowAuthor { set; get; } = false;
            public bool isBlockedAuthor { set; get; } = false;
            public double averageRating { set; get; } = 0;
            public double userRating { set; get; } = 0;
        }
        public record NovelHistoryViewModel
        {
            public Chapter chapterView { set; get; }
            public Novel novelInfo { set; get; }
        }
        public record HistoryViewModel
        {
            public NovelHistoryViewModel history { set; get; }
            public DateTime? timeView { set; get; }
        }
    }
}