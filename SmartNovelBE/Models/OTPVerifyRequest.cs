namespace SmartNovelBE.Models
{
    public class OTPVerifyRequest
    {
        public string Token { set; get; }
        public string TokenRecovery { set; get; }
        public string Email { get; set; }
        public string OTP { set; get; }

    }
}
