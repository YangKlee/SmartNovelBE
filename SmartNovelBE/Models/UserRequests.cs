namespace SmartNovelBE.Models
{
    public class UserRequests
    {
        public record RecoveryPassword(string password, string token);
        public record MailToken(string email, bool active);
    }
}
