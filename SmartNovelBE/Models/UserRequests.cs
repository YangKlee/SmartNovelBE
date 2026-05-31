using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace SmartNovelBE.Models
{
    public class UserRequests
    {
        public record RecoveryPassword(string password, string token);
        public record MailToken(string email, bool active);
        public record ChangeInfoUser(string displayName,  string phone, string? birthday);
        public record changePassword(string oldPassword, string newPassword);
        public record CreateNovelRequest
        {
            public string Title { get; init; } = string.Empty;
            public string? Description { get; init; }
            public string AgeRating { get; init; }
            public string Status { get; init; } = string.Empty;
            public List<string> Genres { get; set; }

            public IFormFile? CoverImage { get; init; }
            public IFormFile? BannerImage { get; init; }
        }
    }
}
