using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartNovelBE.Models;

namespace SmartNovelBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorListController : ControllerBase
    {
        private readonly SmartTruyenDbContext _context;

        public AuthorListController(SmartTruyenDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuthors(
            [FromQuery] AuthorListModel req)
        {
            var query = _context.Users
                .Where(u => u.RoleId == "3");

            if (!string.IsNullOrEmpty(req.Keyword))
            {
                query = query.Where(u =>
                    u.Username.Contains(req.Keyword) ||
                    u.DisplayName.Contains(req.Keyword));
            }

            req.TotalItems = await query.CountAsync();

            req.Authors = (await query
             .OrderBy(u => u.Username)
             .Skip((req.CurrentPage - 1) * req.PageSize)
             .Take(req.PageSize)
             .Select(u => new
             {
                 u.Uid,
                 u.Username,
                 u.DisplayName,
                 u.Email
             })
             .ToListAsync())
             .Cast<object>()
             .ToList();

            req.TotalPages =
                (int)Math.Ceiling((double)req.TotalItems / req.PageSize);

            return Ok(req);
        }
    }
}