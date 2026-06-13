namespace SmartNovelBE.Models
{
    public class AuthorFollowDto
    {
        public string Uid { get; set; } = null!;

        public string DisplayName { get; set; } = null!;

        public string? AvatarUrl { get; set; }

        public int CreatorPoint { get; set; }

        public int TotalNovel { get; set; }

        public int TotalFollower { get; set; }
    }
}
