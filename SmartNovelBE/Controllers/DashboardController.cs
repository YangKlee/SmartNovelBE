using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SmartNovelBE.Models;
using SmartNovelBE.Services;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SmartNovelBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "1,2,3")]
    public class DashboardController : Controller
    {
        private readonly JwtServices _jwtServices;
        private readonly SmartTruyenDbContext _context;
        private readonly MailServices _mailServices;
        private readonly IMemoryCache _cache;
        private readonly MenuDashboardService _menuDashboardService;
        public DashboardController(JwtServices jwtServices, SmartTruyenDbContext context, MailServices mailServices, IMemoryCache cache, MenuDashboardService menuDashboardService)
        {
            _jwtServices = jwtServices;
            _context = context;
            _mailServices = mailServices;
            _cache = cache;
            _menuDashboardService = menuDashboardService;
        }




        [HttpGet("getMenuDashboard")]
        public IActionResult GetMenuByRole()
        {
            var roleId = User.FindFirstValue(ClaimTypes.Role);
            var result = _menuDashboardService.GetMenuDashboardByRole(roleId);
            return Ok(result);
        }
        [Authorize(Roles ="3")]
        [HttpGet("getAllAuthorNovelInfo")]
        public async Task<IActionResult> getAllNovelInfo()
        {
            var uid = User.FindFirst("uid")?.Value;
            var allNovel = await _context.Novels.Where(n => n.Uid == uid).ToListAsync();
            // Lấy trực tiếp các chapter của tác giả này từ DB để tối ưu, không tải toàn bộ Chapters
            var myChapters = await _context.Chapters.Where(c => c.Novel.Uid == uid).ToListAsync();
            var user = await _context.Users.FirstOrDefaultAsync(n => n.Uid == uid);
            var allAuthor = await _context.Users.Where(u => u.RoleId == "3").OrderByDescending(u => u.CreatorPoint).ToListAsync();
            var statsAuthor = new UserRespone.DashboardAuthorStatsNovelViewModel
            {
                TotalNovels = allNovel?.Count(),
                DraftNovels = allNovel?.Count(n => n.Status != null && n.Status.ToLower() == "draft"),
                PublicNovels = allNovel?.Count(n => n.Status != null && n.Status.ToLower() == "public"),
                RemovedNovels = allNovel?.Count(n => n.Status != null && n.Status.ToLower() == "reject"),
                TotalChapters = myChapters.Count,
                PublicChapters = myChapters.Count(c => c.Status != null && c.Status.ToLower() == "public"),
                DraftChapters = myChapters.Count(c => c.Status != null && c.Status.ToLower() == "draft"),
                RemovedChapters = myChapters.Count(c => c.Status != null && (c.Status.ToLower() == "reject")),

            };

            return Ok(statsAuthor);
        }
        [Authorize(Roles = "3")]
        [HttpGet("getAuthorProfileInfo")]
        public async Task<IActionResult> getAuthorProfileInfo()
        {
            var uid = User.FindFirst("uid")?.Value;

            // Tính tổng số view của các truyện do tác giả này đăng
            var totalView = await _context.Novels
                .Where(n => n.Uid == uid)
                .SumAsync(n => (int?)n.ViewCount) ?? 0;

            // Đếm số người theo dõi tác giả
            var followerCount = await _context.Users
                .Where(u => u.Uid == uid)
                .Select(u => u.FollowerUs.Count)
                .FirstOrDefaultAsync();

            var res = new UserRespone.DashboardAuthorStatsProflileViewModel
            {
                countFollower = followerCount,
                totalView = totalView
            };

            return Ok(res);
        }
        [Authorize(Roles = "3")]
        [HttpGet("getNewestComment")]
        public async Task<IActionResult> getNewestComment()
        {
            var uid = User.FindFirst("uid")?.Value;
            var allComment = await _context.Comments
                .Include(c => c.Chapter).ThenInclude(ch => ch.Novel)
                .Include(c => c.UidNavigation)
                .Where(c => c.Chapter.Novel.Uid == uid && c.Uid != uid)
                .OrderByDescending(c => c.TimeCommeny)
                .Take(10)
                .Select(c => new UserRespone.CommentResponse
                {
                    CommentId = c.CommentId,
                    NovelId = c.Chapter.NovelId,
                    ChapterId = c.ChapterId,
                    ParentCommentId = c.ParentCommentId,
                    UserId = c.Uid,
                    Content = c.Content,
                    DisplayName = c.UidNavigation.DisplayName,
                    UserAvatarUrl = c.UidNavigation.AvartarUrl,
                    CommentDateTime = c.TimeCommeny,
                    CurrentUserId = uid,
                    RoleId = c.UidNavigation.RoleId,
                    CountChildComment = c.InverseParentComment.Count
                })
                .ToListAsync();

            var totalComment = await _context.Comments
                .Where(c => c.Chapter.Novel.Uid == uid && c.Uid != uid)
                .CountAsync();

            var model = new UserRespone.CommentReponeReal
            {
                comments = allComment,
                totalComment = totalComment
            };
            
            return Ok(model);
        }

        [Authorize(Roles = "1,2")]
        [HttpGet("getUserStats")]
        public async Task<IActionResult> GetUserStats()
        {
            var totalUsers = await _context.Users.CountAsync();
            var adminCount = await _context.Users.CountAsync(u => u.RoleId == "1");
            var moderatorCount = await _context.Users.CountAsync(u => u.RoleId == "2");
            var authorCount = await _context.Users.CountAsync(u => u.RoleId == "3");
            var readerCount = await _context.Users.CountAsync(u => u.RoleId == "4");

            var activeCount = await _context.Users.CountAsync(u => u.Status != null && u.Status.ToLower() == "active");
            var blockedCount = totalUsers - activeCount;

            var stats = new UserRespone.DashboardUserStatsInfo
            {
                TotalUsers = totalUsers,
                AdminCount = adminCount,
                ModeratorCount = moderatorCount,
                AuthorCount = authorCount,
                ReaderCount = readerCount,
                ActiveCount = activeCount,
                BlockedCount = blockedCount
            };

            return Ok(stats);
        }

        [Authorize(Roles = "1,2")]
        [HttpGet("getActivityStats")]
        public async Task<IActionResult> GetActivityStats()
        {
            var startDate = DateTime.Today.AddDays(-6);

            var novelStats = await _context.Novels
                .Where(n => n.CreateTime.HasValue && n.CreateTime.Value >= startDate)
                .GroupBy(n => n.CreateTime!.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var chapterStats = await _context.Chapters
                .Where(c => c.CreateTime.HasValue && c.CreateTime.Value >= startDate)
                .GroupBy(c => c.CreateTime!.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var novelDict = novelStats.ToDictionary(x => x.Date, x => x.Count);
            var chapterDict = chapterStats.ToDictionary(x => x.Date, x => x.Count);

            var today = DateTime.Today;
            var result = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-i))
                .OrderBy(d => d)
                .Select(date => new UserRespone.DashboardActivityStatsInfo
                {
                    Date = date.ToString("dd/MM"),
                    NovelsAdded = novelDict.TryGetValue(date, out var nc) ? nc : 0,
                    ChaptersAdded = chapterDict.TryGetValue(date, out var cc) ? cc : 0
                })
                .ToList();

            return Ok(result);
        }
    }
}
