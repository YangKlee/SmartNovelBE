using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartNovelBE.Models;

namespace SmartNovelBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly SmartTruyenDbContext _context;

        public CategoriesController(SmartTruyenDbContext context)
        {
            _context = context;
        }

        // API: GET /api/categories
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            // Lấy toàn bộ thể loại từ bảng Category trong DB ra
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }
    }
}