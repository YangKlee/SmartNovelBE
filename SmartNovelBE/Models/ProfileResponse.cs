namespace SmartNovelBE.Models
{
    public class ProfileResponse
    {
        public string Uid { get; set; } = null!;

        public string Username { get; set; } = null!;

        public string DisplayName { get; set; } = null!;

        public string? AvatarUrl { get; set; }

        public string RoleId { get; set; } = null!;

        public string Status { get; set; } = null!;

        public int CreatorPoint { get; set; }

        public int TotalNovel { get; set; }

        public int TotalFollower { get; set; }

        public int TotalFollowingAuthor { get; set; }

        public bool IsFollowing { get; set; }

        public bool IsBlocked { get; set; }

        public List<ProfileNovelDto> Novels { get; set; }
            = new();
    }
}
