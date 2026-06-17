using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Org.BouncyCastle.Ocsp;
using SmartNovelBE.Models;
using SmartNovelBE.Services;
namespace SmartNovelBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly JwtServices _jwtServices;
        private readonly SmartTruyenDbContext _context;
        private readonly MailServices _mailServices;
        private readonly IMemoryCache _cache;
        private readonly IUserRelationService _userRelationService;
        public AccountController(JwtServices jwtServices, SmartTruyenDbContext context, MailServices mailServices, IMemoryCache cache, IUserRelationService userRelationService)
        {
            _jwtServices = jwtServices;
            _context = context;
            _mailServices = mailServices;
            _cache = cache;
            _userRelationService = userRelationService;
        }
        [HttpGet("accountInfo")]
        public async Task<ActionResult<User>> accountInfo()
        {
            // lấy uid từ token
            var uid = User.FindFirst("uid")?.Value;
            if (uid == null)
            {
                return Unauthorized();
            }
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Uid == uid);
            if(user == null)
            {
                return BadRequest();
            }
            return user;
        }
        [HttpPost("updateInfoAccount")]
        public async Task<IActionResult> updateInfoAccount(UserRequests.ChangeInfoUser req)
        {
            var uid = User.FindFirst("uid")?.Value;
            if (uid == null)
                return Unauthorized();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Uid == uid);
            if (user == null)
                return Unauthorized();
            user.DisplayName = req.displayName;
            if(req.birthday != null)
            {
                DateTime bthDate = DateTime.ParseExact(
                    req.birthday,
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture
                );
                user.Birthday = DateOnly.FromDateTime(bthDate);
            }
            user.Phone = req.phone;
            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                var phoneExists = await _context.Users.AnyAsync(x =>
                    x.Phone == req.phone &&
                    x.Uid != uid
                );
                if (phoneExists)
                {
                    return BadRequest(new { Msg = "Số điện thoại đã tồn tại" });

                }
               
            }
            return BadRequest(new { Msg = "Lỗi không xác định" });
        }
        [HttpPost("changePassword")]
        public async Task<IActionResult> changePassword(UserRequests.changePassword req)
        {
            var passhass = new PasswordHasher<object>();
            var uid = User.FindFirst("uid")?.Value;
            if (uid == null)
                return Unauthorized();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Uid == uid);
            if (user == null)
                return Unauthorized();
            var resultHashPassword = passhass.VerifyHashedPassword(
                null,
                user.Password,
                req.oldPassword
            );

            if (resultHashPassword == PasswordVerificationResult.Failed)
            {
                return BadRequest(new { Msg = "Mật khẩu cũ sai" });
            }
            else
            {
                user.Password = passhass.HashPassword(null, req.newPassword);
                await _context.SaveChangesAsync();
                return Ok();
            }
        }

        [HttpGet("profile/{uid}")]
        public async Task<IActionResult> GetProfile(string uid)
        {
            var currentUid = User.FindFirst("uid")?.Value;

            var result = await _userRelationService.GetProfileAsync(currentUid, uid);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}
