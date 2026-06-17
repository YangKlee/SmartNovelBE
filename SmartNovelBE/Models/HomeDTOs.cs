    using System.Collections.Generic;

namespace SmartNovelBE.DTOs.Home
{
   
    public class NovelSummaryDto
    {
        public string NovelId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!; 
        public string? imageNovelUrl { get; set; }
        public string imageBanerNovelUrl { get;  set; }
        public string AuthorName { get; set; } = null!; 
        public string Status { get; set; } = null!;
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
       public string AgeRating { get; set; } = null!;
        public string Description { get; set; }
        public int CountChapter { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
    }

    public class AuthorSummaryDto
    {
        public string Uid { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? AvatarUrl { get; set; } 
        public int CreatorPoint { get; set; }
    }

}