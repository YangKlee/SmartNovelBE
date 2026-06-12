using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SmartNovelBE.Models;
using SmartNovelBE.Services;
using System.Security.Claims;

namespace SmartNovelBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "1,2,3")]
    public class DashboardServices : Controller
    {
        private readonly JwtServices _jwtServices;
        private readonly SmartTruyenDbContext _context;
        private readonly MailServices _mailServices;
        private readonly IMemoryCache _cache;
        private readonly MenuDashboardService _menuDashboardService;
        public DashboardServices(JwtServices jwtServices, SmartTruyenDbContext context, MailServices mailServices, IMemoryCache cache, MenuDashboardService menuDashboardService)
        {
            _jwtServices = jwtServices;
            _context = context;
            _mailServices = mailServices;
            _cache = cache;
            _menuDashboardService = menuDashboardService;
        }




        [HttpGet("getMenuDashboard")]
        public IActionResult GetMenuByRole()
        {
            var roleId = User.FindFirstValue(ClaimTypes.Role);
            var result = _menuDashboardService.GetMenuDashboardByRole(roleId);
            return Ok(result);
        }

    }
}
