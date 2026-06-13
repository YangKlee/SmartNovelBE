namespace SmartNovelBE.Models
{
    public class BlockedUserDto
    {
        public string Uid { get; set; } = null!;

        public string Username { get; set; } = null!;

        public string DisplayName { get; set; } = null!;

        public string? AvatarUrl { get; set; }

        public string RoleId { get; set; } = null!;

        public string Status { get; set; } = null!;
    }
}
