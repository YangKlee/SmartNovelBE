namespace SmartNovelBE.Models
{
    public class LoginRespone
    {
        public string? username { set; get; }
        public string? UID { set; get; }
        public string? roleID { set; get; }
        public string? token { set; get; }
        public int expiresIn { set; get; }
    }
}
