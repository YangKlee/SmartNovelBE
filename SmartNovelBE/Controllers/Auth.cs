using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Common;
using Org.BouncyCastle.Ocsp;
using SmartNovelBE.Models;
using SmartNovelBE.Services;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace SmartNovelBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Auth : Controller
    {
        private readonly JwtServices _jwtServices;
        private readonly SmartTruyenDbContext _context;
        private readonly MailServices _mailServices;
        private readonly IMemoryCache _cache;
        public Auth(JwtServices jwtServices, SmartTruyenDbContext context, MailServices mailServices, IMemoryCache cache)
        {
            _jwtServices = jwtServices;
            _context = context;
            _mailServices = mailServices;
            _cache = cache;
        }
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<LoginRespone>> Login(LoginRequest req)
        {
            var result = await _jwtServices.Authenticate(req);
            if (result is null)
            {
                return Unauthorized();
            }
            return result;
        }
        [AllowAnonymous]
        [HttpPost("SendEmailForgotPassword")]
        public async Task<ActionResult<OTPVerifyRepone>> sendOTPFogotPassword(OTPVerifyRequest otpemail)
        {
            // chặn gửi mail nhiều lần
            bool isExist = _cache.TryGetValue(otpemail.Email, out int a);
            if(isExist)
            {
                return BadRequest(new OTPVerifyRepone
                {
                    code = 400,
                    content = "Bạn không được gửi email quá nhiều lần, vui lòng chờ nha!",
                    token = null
                });
            }
            var findAccountEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == otpemail.Email);
            if(findAccountEmail == null)
            {

                return BadRequest(new OTPVerifyRepone
                {
                    code = 400,
                    content = "Gửi otp thất bại, không tìm thấy tài khoản với địa chỉ email đó!",
                    token = null
                });
            }
            Random rnd = new Random();
            var optCode = (rnd.Next(1000, 9999));
            var body = $"Mã xác minh của người dùng {findAccountEmail.Username} là {optCode}";
            var isSuccessMail = await _mailServices.SendEmailAsync(otpemail.Email, "OTP khôi phục mật khẩu SmartNovel", body);
            if(isSuccessMail)
            {
                var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                var cacheOptionsForSpamEmail = 
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
                Guid token = Guid.NewGuid();
                var otpData = new OtpInfo
                {
                    code = optCode.ToString(),
                    attemp = 5
                };
                _cache.Set(otpemail.Email, 1 , cacheOptionsForSpamEmail);
                _cache.Set(token.ToString(), otpData, cacheOptions);

                return Ok(new OTPVerifyRepone
                {
                    code = 200,
                    content = "Gửi otp thành công, mã có tác dụng trong 5 phút!",
                    token = token.ToString()
                });

            }


            return BadRequest(new OTPVerifyRepone
            {
                code = 400,
                content = "Gửi otp không thành công, vui lòng thử lại sau",
                token = null
            });

        }
        [AllowAnonymous]
        [HttpPost("VerifyOTP")]
        public async Task<IActionResult> verifyOTP(OTPVerifyRequest otpreq)
        {

            bool isExist = _cache.TryGetValue(otpreq.Token, out OtpInfo otpInfo);
            if (isExist)
            {
               if(otpInfo.attemp <= 0)
                {
                    _cache.Remove(otpreq.Token);
                    return BadRequest(new OTPVerifyRepone { code=400, content="Nhập OTP sai quá nhiều lần"});
                }
               if(otpreq.OTP == otpInfo.code)
                {
                    _cache.Remove(otpreq.Token);
                    return Ok(new OTPVerifyRepone { code = 200, content = "Thành công" });


                }
                otpInfo.attemp -= 1;

            }
            return BadRequest(new OTPVerifyRepone { code = 400, content = "OTP hết hạn" });
        }
        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<User>> profile()
        {
            // lấy uid từ token
            var uid = User.FindFirst("uid")?.Value;

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Uid == uid);
            return user;
        }
        [AllowAnonymous]
        [HttpPost("Regist")]
        public async Task<ActionResult> Regist(RegistRequest req)
        {
            var passwordHasher = new PasswordHasher<object>();
            var user = new User();
            user.Username = req.Username;

            user.DisplayName = req.DisplayName;

            user.Email = req.Email;
            user.Phone = req.Phone;

            user.Uid = Guid.NewGuid().ToString();
            user.Password = passwordHasher.HashPassword(user,req.Password);
            user.RoleId = "4"; // gán user ban đầu là độc giả
            user.Status = "Active";
            _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync();
                return Ok(new 
                {
                    content = "Đăng ký thành công rồi nè!",
                });
            }
            catch 
            {
                var checkUsername = await _context.Users.FirstOrDefaultAsync(x => x.Username == user.Username);
                var checkEmail = _context.Users.FirstOrDefaultAsync(x => x.Email == user.Email);
                var checkPhone = _context.Users.FirstOrDefaultAsync(x => x.Phone == user.Phone);
                if (checkUsername != null)
                {
                    return BadRequest(new
                    {
                        content = "Username đã tồn tại!",
                    });
                }
                else if (checkEmail != null)
                {
                    return BadRequest(new
                    {
                        content = "Email đã tồn tại!",
                    });
                }
                else if (checkPhone != null)
                {
                    return BadRequest(new
                    {
                        content = "Số điện thoại đã tồn tại!",
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        content = "Lỗi không xác định!",
                    });
                }
                
            }


        }
    }
}
