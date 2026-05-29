using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace SmartNovelBE.Models
{
    public class UserRequests
    {
        public record RecoveryPassword(string password, string token);
        public record MailToken(string email, bool active);
        public record ChangeInfoUser(string displayName,  string phone, string? birthday);
    }
}
