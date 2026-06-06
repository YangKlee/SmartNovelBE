using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartNovelBE.Models;

namespace SmartNovelBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly SmartTruyenDbContext _context;

        public CategoriesController(SmartTruyenDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // API TỪ NHÁNH TIM_KIEM
        // ==========================================
        
        // API: GET /api/categories
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            // Lấy toàn bộ thể loại từ bảng Category trong DB ra
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }

        // ==========================================
        // API TỪ NHÁNH SPRINT1-DEV
        // ==========================================

        // GET: api/Categories/Active
        [HttpGet("Active")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategoriesActive()
        {
            return await _context.Categories.Where(c => c.Status == "ACTIVE").OrderBy(c => c.Name).ToListAsync();
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(string id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // PUT: api/Categories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(string id, Category category)
        {
            if (id != category.CategoryId)
            {
                return BadRequest();
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Categories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            _context.Categories.Add(category);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CategoryExists(category.CategoryId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCategory", new { id = category.CategoryId }, category);
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(string id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}