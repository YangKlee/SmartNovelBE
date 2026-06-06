namespace SmartNovelBE.Models
{
    public class UserListDto
    {
        public string Uid { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string RoleId { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int? CreatorPoint { get; set; }
        public DateOnly? Birthday { get; set; }


    }

}


