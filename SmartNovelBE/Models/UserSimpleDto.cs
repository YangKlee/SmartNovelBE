namespace SmartNovelBE.DTOs.User
{
    public class UserSimpleDto
    {
        public string Uid { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
    }
}
