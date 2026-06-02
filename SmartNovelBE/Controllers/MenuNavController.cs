using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNovelBE.Models;
using Microsoft.EntityFrameworkCore;
namespace SmartNovelBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuNavController : ControllerBase
    {
        private readonly SmartTruyenDbContext _context;

        public MenuNavController(SmartTruyenDbContext context)
        {
            _context = context;
        }

        // api/MenuNav/4
        [AllowAnonymous]
        [HttpGet("{roleId}")]
        public async Task<ActionResult<List<MenuNavDTO>>> GetMenuByRole(string roleId)
        {
            var menus = await _context.MenuNavs
                .Where(x => x.RoleId == roleId)
                .OrderBy(x => x.Id)
                .ToListAsync();

            var result = BuildMenuTree(menus, null);

            return Ok(result);
        }

        private List<MenuNavDTO> BuildMenuTree(List<MenuNav> menus, int? parentId)
        {
            return menus
                .Where(x => x.ParentId == parentId)
                .Select(x => new MenuNavDTO
                {
                    ID = x.Id,
                    Content = x.Content,
                    IconBootstrap = x.IconBootstrap,
                    UrlLink = x.UrlLink,
                    Slots = x.Slots,

                    Children = BuildMenuTree(menus, x.Id)
                })
                .ToList();
        }
    }
}
