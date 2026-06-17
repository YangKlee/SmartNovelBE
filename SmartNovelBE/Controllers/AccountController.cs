using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Org.BouncyCastle.Ocsp;
using SmartNovelBE.Models;
using SmartNovelBE.Services;
using System.Security.Claims;
using static SmartNovelBE.Models.UserRespone;
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
        [Authorize]
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
        [Authorize]
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
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var currentUid = User.FindFirst("uid")?.Value;

            var result = await _userRelationService.GetProfileAsync(currentUid, currentUid);

            if (result == null)
                return NotFound();

            return Ok(result);
        [Authorize]
        [HttpPost("changeAvatar")]
        public async Task<IActionResult> changeProfileImage([FromForm] UserRequests.uploadAvatar req)
        {
            var uid = User.FindFirst("uid")?.Value;
            string publicLink = "https://pub-20056e4912f440f08b3d40eea545f95f.r2.dev/smart-novel/user-image/";
            long maxFileSizeInBytes = 5 * 1024 * 1024;
            string[] permittedExtensions = { ".jpg", ".jpeg", ".png" };
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == uid);
            if (req.NewImage != null)
            {
                var extension = Path.GetExtension(req.NewImage.FileName).ToLowerInvariant();
                if (req.NewImage.Length > maxFileSizeInBytes && req.NewImage.Length == 0)
                {

                    return BadRequest(new { Msg = "File upload phải > 0b đến <= 5mb" });
                }


                if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
                {

                    return BadRequest(new { Msg = "Loại file không hợp lệ" });
                }
                if (!string.IsNullOrEmpty(user.AvartarUrl))
                {
                    // xóa file cũ
                    var fileOldName = user.AvartarUrl.Replace(publicLink, "");
                    await _fileServicesUpload.DeleteFile("smart-novel/user-image/", fileOldName);
                }
                string fileName = Guid.NewGuid().ToString() + "-" + req.NewImage.FileName;
                var res = await _fileServicesUpload.UploadFile("smart-novel/user-image/", fileName, req.NewImage);
                if (res)
                {


                    user.AvartarUrl = publicLink + fileName;
                    await _context.SaveChangesAsync();


                    return Ok();

                }

                return BadRequest(new { Msg = "Lỗi không xác định" });
            }
            else
            {
                return BadRequest(new { Msg = "Lỗi không xác định" });
            }


        }

        [Authorize]
        [HttpPost("change-author")]     
        public async Task<IActionResult> dochangeAuthor()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var uid = User.FindFirst("uid")?.Value;

            if (role == "4")
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == uid);
                if (user == null)
                {
                    return NotFound(new { Msg = "Không tìm thấy user" });
                }

                user.RoleId = "3";
                await _context.SaveChangesAsync();

                // Tạo lại token với quyền mới
                var newToken = _jwtServices.GenerateToken(user);

                return Ok(new 
                { 
                    Msg = "Chuyển tác giả thành công!",
                    Token = newToken 
                });
            }
            else
            {
                return BadRequest(new { Msg = "Không thể chuyển tác giả hoặc bạn đã là tác giả!" });
            }
        }
        [HttpGet("getHistoryView")]
        [Authorize]
        public async Task<IActionResult> HistoryViewUser()
        {
            var uid = User.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(uid))
            {
                return Unauthorized();
            }

            var history = await _context.HistoryReaders
                    .Include(h => h.Novel)
                    .Include(h => h.Chapter)
                    .Where(h => h.Uid == uid)
                    .OrderByDescending(h => h.TimeReader)
                    .ToListAsync();

            var model = history.Select(h => new UserRespone.HistoryViewModel
            {
                history = new UserRespone.NovelHistoryViewModel
                {
                    chapterView = h.Chapter,
                    novelInfo = h.Novel
                },
                timeView = h.TimeReader
            }).ToList();

            return Ok(model);
        }
    }
}
