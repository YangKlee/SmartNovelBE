using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartNovelBE.Models;

namespace SmartNovelBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NovelsController : ControllerBase
    {
        private readonly SmartTruyenDbContext _context;

        public NovelsController(SmartTruyenDbContext context)
        {
            _context = context;
        }
       

        [HttpGet("filter")]
        public async Task<IActionResult> FilterNovels(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int? minChapters,
        [FromQuery] string? sortBy,
        [FromQuery] string? categoryId,
        [FromQuery] string? authorId,      // Bổ sung nhận mã tác giả
        [FromQuery] double? minRating,     // Bổ sung nhận điểm rating tối thiểu
        [FromQuery] string? currentUid)
        {
            // 1. Tạo Query cơ sở, bao gồm tính luôn điểm Rating Trung Bình của từng truyện
            var query = _context.Novels.Select(n => new {
                Novel = n,
                AuthorName = _context.Users.Where(u => u.Uid == n.Uid).Select(u => u.DisplayName).FirstOrDefault(),
                ChapterCount = _context.Chapters.Count(c => c.NovelId == n.NovelId),
                AverageRating = _context.Ratings.Where(r => r.NovelId == n.NovelId).Average(r => (double?)r.RatingPoint) ?? 0.0
            });

            // 2. Áp dụng các bộ lọc cơ bản
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(q =>
                    q.Novel.Title.ToLower().StartsWith(keyword));
            }
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(q => q.Novel.Status == status);
            }

            // SỬA CHỖ NÀY: Xử lý dải chương (1: 1-50, 2: 51-100, 3: 101-150)
            if (minChapters > 0)
            {
                if (minChapters == 1) query = query.Where(q => q.ChapterCount >= 1 && q.ChapterCount <= 50);
                else if (minChapters == 2) query = query.Where(q => q.ChapterCount >= 51 && q.ChapterCount <= 100);
                else if (minChapters == 3) query = query.Where(q => q.ChapterCount >= 101 && q.ChapterCount <= 150);
            }

            // 3. XỬ LÝ NHÁNH LỌC ĐỘNG
            if (sortBy == "category" && !string.IsNullOrEmpty(categoryId))
            {
                var catIds = categoryId.Split(',');
                query = query.Where(q => q.Novel.Categories.Any(c => catIds.Contains(c.CategoryId)));
            }
            else if (sortBy == "author" && !string.IsNullOrEmpty(authorId))
            {
                query = query.Where(q => q.Novel.Uid == authorId);
            }
            else if (sortBy == "rating" && minRating > 0)
            {
                query = query.Where(q => q.AverageRating >= minRating);
            }
            // 4. KIỂM TRA SẮP XẾP ĐẦU RA
            if (sortBy == "rating")
            {
                query = query.OrderByDescending(q => q.AverageRating); // Rating cao xếp trước
            }
            else
            {
                query = query.OrderByDescending(q => q.Novel.UpdateTime); // Mặc định xếp theo ngày cập nhật
            }

            // 5. Trả về cấu trúc dữ liệu gọn sạch cho Angular nhận diện
            var result = await query.Select(q => new {
                novelId = q.Novel.NovelId,
                title = q.Novel.Title,
                status = q.Novel.Status,
                uid = q.Novel.Uid,
                authorName = q.AuthorName,
                averageRating = q.AverageRating,
                chapterCount = q.ChapterCount
            }).ToListAsync();

            return Ok(result);
        }
    }
}