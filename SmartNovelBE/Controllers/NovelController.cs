using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Common;
using Org.BouncyCastle.Ocsp;
using SmartNovelBE.Models;
using SmartNovelBE.Services;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace SmartNovelBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NovelController : ControllerBase
    {

        private readonly JwtServices _jwtServices;
        private readonly SmartTruyenDbContext _context;
        private readonly MailServices _mailServices;
        private readonly IMemoryCache _cache;
        public NovelController(JwtServices jwtServices, SmartTruyenDbContext context, MailServices mailServices, IMemoryCache cache)
        {
            _jwtServices = jwtServices;
            _context = context;
            _mailServices = mailServices;
            _cache = cache;
        }
        [HttpGet("getUserNovel")]
        [Authorize]
        public async Task<IActionResult> getListUserNovel()
        {
            var uid = User.FindFirst("uid")?.Value;
            if (uid == null)
                return Unauthorized();
            var novels = await _context.Novels
                .Where(n => n.Uid == uid)
                .Select(n => new
                {
                    NovelId = n.NovelId,
                    Title = n.Title,
                    Slug = n.Slug,
                    Description = n.Description,
                    AgeRating = n.AgeRating,
                    ImageNovelUrl = n.ImageNovelUrl,
                    ImageBanerNovelUrl = n.ImageBanerNovelUrl,
                    Status = n.Status,
                    ViewCount = n.ViewCount,
                    LikeCount = n.LikeCount,
                    CreateTime = n.CreateTime,
                    UpdateTime = n.UpdateTime,

                    countChapter = n.Chapters.Count(),
                    countChapterPublic = n.Chapters.Count(c => c.Status == "Public"),
                    countChapterDraf = n.Chapters.Count(c => c.Status == "Draft"),
                    countChapterRemove = n.Chapters.Count(c => c.Status == "Cancel"),
                    novelRating = n.Ratings
                    .Select(r => (double?)r.RatingPoint)
                    .Average() ?? 0
                })
                .ToListAsync();
            return Ok(novels);
        }
        [HttpGet("getInfoNovel/{id}")]
        public async Task<IActionResult> getInfoNovel(string id)
        {
            var novelBasicInfo = await _context.Novels.FirstOrDefaultAsync(x => x.NovelId == id);
            var authorNovelInfo = await _context.Novels.Where(n => n.NovelId == id).Join(_context.Users, n => n.Uid, u => u.Uid,
                (n, u) => new
                {
                    u.DisplayName,
                    u.AvartarUrl,
                }).ToListAsync();
            {

                if (novelBasicInfo == null)
                {
                    return BadRequest(new
                    {
                        msg = "Không tìm thấy truyện",
                    });
                }
                return Ok(new
                {
                    novelBasicInfo,
                    authorNovelInfo

                });
            }
        }
    }
}
