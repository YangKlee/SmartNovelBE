using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartNovelBE.Models;
using SmartNovelBE.Services;

namespace SmartNovelBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Auth : Controller
    {
        private readonly JwtServices _jwtServices;
        private readonly SmartTruyenDbContext _context;
        public Auth(JwtServices jwtServices, SmartTruyenDbContext context)
        {
            _jwtServices = jwtServices;
            _context = context;
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
        public async Task<ActionResult<User>> Regist(RegistRequest req)
        {
            var passwordHasher = new PasswordHasher<object>();
            var user = new User();
            user.Username = req.Username;

            user.DisplayName = req.DisplayName;

            user.Email = req.Email;
            user.Phone = req.Phone;

            user.Uid = Guid.NewGuid().ToString();
            user.Password = passwordHasher.HashPassword(user,req.Password);
            user.RoleId = "4";
            user.Status = "Active";
            _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var checkUsername = _context.Users.Any(x => x.Username == user.Username);
                if (checkUsername)
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Ok(user);
        }
    }
}
