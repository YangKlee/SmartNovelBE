using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace SmartNovelBE.Models
{
    public class UserRequests
    {
        public record RecoveryPassword(string password, string token);
        public record MailToken(string email, bool active);
        public record ChangeInfoUser(string displayName,  string phone, string? birthday);
        public record changePassword(string oldPassword, string newPassword);
        public record UpdateReadingPreferences(string theme, int fontSize, string fontFamily);
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
        public record ModifyNovelRequest
        {
            public string NovelID { get; set; }
            public string Title { get; init; } = string.Empty;
            public string? Description { get; init; }
            public string AgeRating { get; init; }
            public string Status { get; init; } = string.Empty;
            public List<string> Genres { get; set; }

            public IFormFile? CoverImage { get; init; }
            public IFormFile? BannerImage { get; init; }
        }
        public record chapterCreate(string title, int oder, string decrip,
            string status, bool allowComment, string content);

        public record searchNovel(string status, string? keyworld);
        public record searchChapter(string novelID,string status, string? keyworld);

    }
}
