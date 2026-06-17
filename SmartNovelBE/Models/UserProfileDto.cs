using SmartNovelBE.DTOs.Novel;

namespace SmartNovelBE.DTOs.User
{
    public class UserProfileDto
    {
        public string Uid { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
        public string RoleId { get; set; }
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        // role 3
        public long TotalViews { get; set; }
        public int NovelCount { get; set; }
        public List<NovelListDto> Novels { get; set; }
        // role 3 + 4
        public List<NovelListDto> FollowedNovels { get; set; }

        public List<UserSimpleDto> Followers { get; set; }
        public List<UserSimpleDto> Following { get; set; }

        public bool IsSelf { get; set; }
        public bool IsFollowing { get; set; }
        public bool IsBlocked { get; set; }
    }
}
