using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SmartNovelBE.Models;
using SmartNovelBE.Services;
using Microsoft.EntityFrameworkCore;
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
        public AccountController(JwtServices jwtServices, SmartTruyenDbContext context, MailServices mailServices, IMemoryCache cache)
        {
            _jwtServices = jwtServices;
            _context = context;
            _mailServices = mailServices;
            _cache = cache;
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
    }
}
