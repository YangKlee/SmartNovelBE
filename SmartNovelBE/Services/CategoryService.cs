using Microsoft.EntityFrameworkCore;
using SmartNovelBE.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartNovelBE.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly SmartTruyenDbContext _context;

        public CategoryService(SmartTruyenDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetCategoriesAsync(string? keyword, string? status, int page)
        {
            int pageSize = 10;
            var query = _context.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(c => c.Name.Contains(keyword));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);

            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            
            var categories = await query
                .OrderByDescending(c => c.CategoryId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new { data = categories, currentPage = page, totalPages = totalPages, totalRecords = totalRecords };
        }

        public async Task<Category> CreateCategoryAsync(Category model)
        {
            bool isExists = await _context.Categories.AnyAsync(c => c.Name == model.Name);
            if (isExists) throw new Exception("Tên thể loại đã tồn tại!");
            
            string newId;
            bool isIdTaken;
            Random rnd = new Random();
            do
            {
                newId = rnd.Next(1, int.MaxValue).ToString();
                isIdTaken = await _context.Categories.AnyAsync(c => c.CategoryId == newId);
            } while (isIdTaken);

            model.CategoryId = newId;
            model.Slug = model.Name.ToLower().Replace(" ", "-");
            
            _context.Categories.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<Category> UpdateCategoryAsync(Category model)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == model.CategoryId);
            if (category == null) throw new Exception("Không thấy thể loại!");

            bool nameConflict = await _context.Categories.AnyAsync(c => c.Name == model.Name && c.CategoryId != model.CategoryId);
            if (nameConflict) throw new Exception("Tên thể loại đã bị trùng với thể loại khác!");

            category.Name = model.Name;
            category.Description = model.Description;
            category.Slug = model.Name.ToLower().Replace(" ", "-");
            category.Status = model.Status;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(string id)
        {
            var category = await _context.Categories
                .Include(c => c.Novels)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
                
            if (category == null) return false;

            if (category.Novels.Any())
            {
                throw new Exception($"Không thể xóa! Thể loại này đang chứa {category.Novels.Count} cuốn truyện.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
