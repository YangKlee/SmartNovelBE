using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartNovelBE.Models;

namespace SmartNovelBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryNovelController : ControllerBase
    {
        private readonly SmartTruyenDbContext _context;

        public CategoryNovelController(
            SmartTruyenDbContext context)
        {
            _context = context;
        }

        [HttpGet("{slug}")]
         public async Task<IActionResult> GetNovelByCategory(
         string slug,
         [FromQuery] int currentPage = 1,
         [FromQuery] int pageSize = 12)
            {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Slug == slug);

            if (!categoryExists)
            {
                return NotFound("Không tìm thấy thể loại");
            }

            var query = _context.Categories
                .Where(c => c.Slug == slug)
                .SelectMany(c => c.Novels);

            var totalItems = await query.CountAsync();

            var novels = await query
                .OrderByDescending(n => n.CreateTime)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new
                {
                    n.NovelId,
                    n.Title,
                    n.Slug,
                    n.ImageNovelUrl,
                    n.ViewCount,
                    n.LikeCount
                })
                .ToListAsync();

            return Ok(new
            {
                slug,
                currentPage,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling(
                    (double)totalItems / pageSize),
                novels
            });
        }
    }
}