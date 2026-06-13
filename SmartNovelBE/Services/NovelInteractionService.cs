using Microsoft.EntityFrameworkCore;
using SmartNovelBE.Controllers;
using SmartNovelBE.Models;

namespace SmartNovelBE.Services
{
    public class NovelInteractionService : INovelInteractionService
    {
        private readonly SmartTruyenDbContext _context;

        public NovelInteractionService(
            SmartTruyenDbContext context)
        {
            _context = context;
        }

        public async Task<bool> FollowNovelAsync(string currentUid, string novelId)
        {
            var user = await _context.Users
               .Include(x => x.Novels)
               .FirstOrDefaultAsync(x => x.Uid == currentUid);

            var novel = await _context.Novels
                .FirstOrDefaultAsync(x => x.NovelId == novelId);

            if (user == null || novel == null)
                return false;

            if (user.Novels.Any(x => x.NovelId == novelId))
                return true;

            user.Novels.Add(novel);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UnFollowNovelAsync(
            string currentUid,
            string novelId)
        {
            var user = await _context.Users
       .Include(x => x.Novels)
       .FirstOrDefaultAsync(x => x.Uid == currentUid);

            if (user == null)
                return false;

            var novel = user.Novels
                .FirstOrDefault(x => x.NovelId == novelId);

            if (novel == null)
                return false;

            user.Novels.Remove(novel);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<FollowingNovelDto>> GetFollowingNovelsAsync(string currentUid)
        {
            return await _context.Users
            .Where(x => x.Uid == currentUid)
            .SelectMany(x => x.Novels)
            .Select(n => new FollowingNovelDto
            {
                NovelId = n.NovelId,
                Title = n.Title,
                Slug = n.Slug,
                ImageNovelUrl = n.ImageNovelUrl,
                AuthorName = n.UidNavigation.DisplayName,
                Status = n.Status,
                ViewCount = n.ViewCount ?? 0,
                LikeCount = n.LikeCount ?? 0,

                AverageRating =
                    n.Ratings.Any()
                    ? n.Ratings.Average(r => r.RatingPoint)
                    : 0,

                TotalChapter =
                    n.Chapters.Count()
            })
            .ToListAsync();
        }

        public async Task<RateNovelResponse> RateNovelAsync(string currentUid, RateNovelRequest request)
        {
            var novel = await _context.Novels
    .FirstOrDefaultAsync(x => x.NovelId == request.NovelId);

            if (novel == null)
            {
                return new RateNovelResponse
                {
                    Success = false,
                    Message = "Không tìm thấy truyện"
                };
            }

            if (request.RatingValue < 1 || request.RatingValue > 5)
            {
                return new RateNovelResponse
                {
                    Success = false,
                    Message = "Điểm đánh giá phải từ 1 đến 5"
                };
            }

            // add or update rating
            var existing = await _context.Ratings
                .FirstOrDefaultAsync(r => r.NovelId == request.NovelId && r.Uid == currentUid);

            if (existing != null)
            {
                existing.RatingPoint = request.RatingValue;
                _context.Ratings.Update(existing);
            }
            else
            {
                var rating = new Rating
                {
                    Uid = currentUid,
                    NovelId = request.NovelId,
                    RatingPoint = request.RatingValue
                };

                await _context.Ratings.AddAsync(rating);
            }

            await _context.SaveChangesAsync();

            var totalRatings = await _context.Ratings
                .CountAsync(r => r.NovelId == request.NovelId);

            var average = totalRatings > 0
                ? await _context.Ratings
                    .Where(r => r.NovelId == request.NovelId)
                    .AverageAsync(r => r.RatingPoint)
                : 0;

            return new RateNovelResponse
            {
                Success = true,
                Message = "Đánh giá thành công",
                AverageRating = average,
                TotalRatings = totalRatings
            };
        }
    }
}
