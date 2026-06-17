using Microsoft.EntityFrameworkCore;
using SmartNovelBE.DTOs.Chapter;
using SmartNovelBE.DTOs.Novel;
using SmartNovelBE.Models;
using SSmartNovelBE.Services.Interfaces;

namespace SmartNovelBE.Services
{
    public class NovelService : INovelService
    {
        private readonly SmartTruyenDbContext _context;

        public NovelService(SmartTruyenDbContext context)
        {
            _context = context;
        }

        public async Task<NovelDetailDto?> GetByNovelIdAsync(string novelId)
        {
            var novel = await _context.Novels
                .Include(x => x.UidNavigation)
                .Include(x => x.Categories)
                .Include(x => x.Chapters)
                .Include(x => x.Uids)
                .FirstOrDefaultAsync(x => x.NovelId == novelId);
            if (novel == null)
                return null;

            return new NovelDetailDto
            {
                NovelId = novel.NovelId,
                Title = novel.Title,
                Slug = novel.Slug,
                Description = novel.Description,
                AgeRating = novel.AgeRating,
                imageNovelUrl = novel.ImageNovelUrl,
                ImageBanerNovelUrl = novel.ImageBanerNovelUrl,
                Status = novel.Status,
                AuthorId = novel.Uid,
                AuthorName = novel.UidNavigation.DisplayName,
                ViewCount = novel.ViewCount ?? 0,
                LikeCount = novel.LikeCount ?? 0,
                CreateTime = novel.CreateTime,
                UpdateTime = novel.UpdateTime,
                TotalChapters = novel.Chapters.Count,
                FollowCount = novel.Uids.Count,
                AverageRating = await _context.Ratings
                .Where(x => x.NovelId == novel.NovelId)
                .AverageAsync(x => (double?)x.RatingPoint) ?? 0,
                TotalRatings = await _context.Ratings
                .CountAsync(x => x.NovelId == novel.NovelId),
                Categories = novel.Categories.Select(x => x.Name).ToList()
            };
        }

        public async Task<List<ChapterListDto>> GetChaptersByNovelIdAsync(string novelId)
        {
            return await _context.Chapters
                .Where(x => x.NovelId == novelId && x.Status.ToLower() == "public")
                .OrderBy(x => x.ChaperOrder)
                .Select(x => new ChapterListDto
                {
                    ChapterId = x.ChapterId,
                    ChaperOrder = x.ChaperOrder,
                    ChapterTitle = x.ChapterTitle,
                    CreateTime = x.CreateTime
                })
                .ToListAsync();
        }
    }
}
