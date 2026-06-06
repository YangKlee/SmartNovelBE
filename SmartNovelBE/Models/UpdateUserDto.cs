using System.ComponentModel.DataAnnotations;

namespace SmartNovelBE.DTOs.AdminUser
{
    public class UpdateUserDto
    {
        
        public string Uid { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [MaxLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        public string DisplayName { get; set; } 

        public string? Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ Email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
        public string? Password { get; set; } 

        [Required(ErrorMessage = "Vui lòng chọn quyền cho người dùng.")]
        public string RoleId { get; set; } 

        [Required(ErrorMessage = "Vui lòng chọn trạng thái cho người dùng.")]
        public string Status { get; set; }

        public string? Phone { get; set; }

        public int? CreatorPoint { get; set; }
    }
}