using Microsoft.EntityFrameworkCore;
using SmartNovelBE.DTOs.Novel;
using SmartNovelBE.Models;
using SSmartNovelBE.Services.Interfaces;
using SmartNovelBE.DTOs.Chapter;

namespace SmartNovelBE.Services
{
    public class NovelService : INovelService
    {
        private readonly SmartTruyenDbContext _context;

        public NovelService(
            SmartTruyenDbContext context)
        {
            _context = context;
        }

        public async Task<NovelDetailDto?> GetBySlugAsync(string slug)
        {
            var novel = await _context.Novels

                .Include(x => x.UidNavigation)

                .Include(x => x.Categories)

                .FirstOrDefaultAsync(x => x.Slug == slug);

            if (novel == null)
                return null;

            return new NovelDetailDto
            {
                NovelId = novel.NovelId,

                Title = novel.Title,

                Slug = novel.Slug,

                Description = novel.Description,

                ImageNovelUrl = novel.ImageNovelUrl,

                ImageBanerNovelUrl = novel.ImageBanerNovelUrl,

                AuthorName =
                    novel.UidNavigation.DisplayName,

                ViewCount =
                    novel.ViewCount ?? 0,

                LikeCount =
                    novel.LikeCount ?? 0,

                Categories =
                    novel.Categories
                        .Select(x => x.Name)
                        .ToList()
            };
        }

        public async Task<List<ChapterListDto>> GetChaptersByNovelIdAsync(string novelId)
        {
            return await _context.Chapters

                .Where(x =>
                    x.NovelId == novelId
                    &&
                    x.Status == "ACTIVE")

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
