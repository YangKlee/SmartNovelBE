using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SmartNovelBE.Models;
using SmartNovelBE.Services;
using System.Security.Claims;

namespace SmartNovelBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FollowController : Controller
    {
        private readonly JwtServices _jwtServices;
        private readonly SmartTruyenDbContext _context;
        private readonly MailServices _mailServices;
        private readonly IMemoryCache _cache;
        public FollowController(JwtServices jwtServices, SmartTruyenDbContext context, MailServices mailServices, IMemoryCache cache)
        {
            _jwtServices = jwtServices;
            _context = context;
            _mailServices = mailServices;
            _cache = cache;
        }

        [Authorize]
        [HttpGet("followNovel/{novelId}")]
        public async Task<IActionResult> FollowNovel(string novelId)
        {
            var uid = User.FindFirst("uid")?.Value;

            var user = _context.Users
                .Include(x => x.Novels)
                .FirstOrDefault(x => x.Uid == uid);

            var novel = _context.Novels
                .FirstOrDefault(x => x.NovelId == novelId);

            if (user == null || novel == null)
                return NotFound();

            if (!user.Novels.Any(x => x.NovelId == novelId))
            {
                user.Novels.Add(novel);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
        [Authorize]
        [HttpGet("unFollowNovel/{novelId}")]
        public async Task<IActionResult> UnFollowNovel(string novelId)
        {
            var uid = User.FindFirst("uid")?.Value;

            var user = _context.Users
                .Include(x => x.Novels)
                .FirstOrDefault(x => x.Uid == uid);

            if (user == null)
                return NotFound();

            var novel = user.Novels
                .FirstOrDefault(x => x.NovelId == novelId);

            if (novel != null)
            {
                user.Novels.Remove(novel);
                await _context.SaveChangesAsync();


            }
            return Ok();

        }
        [HttpPost("followAuthor")]
        [Authorize]
        public async Task<IActionResult> followAuthor([FromForm] string authorId, [FromForm] string novelID)
        {
            var uid = User.FindFirst("uid")?.Value;
            var author = await _context.Users.FirstOrDefaultAsync(u => u.Uid == authorId);
            var user = await _context.Users.Include(u => u.UidsNavigation).FirstOrDefaultAsync(u => u.Uid == uid);

            if (author == null || user == null)
                return NotFound();
            if (!user.UidsNavigation.Any(u => u.Uid == authorId))
            {
                user.UidsNavigation.Add(author);
                await _context.SaveChangesAsync();
            }
            return Ok();

        }
        [HttpPost("unFollowAuthor")]
        [Authorize]
        public async Task<IActionResult> unFollowAuthor([FromForm] string authorId, [FromForm] string? novelID)
        {
            var uid = User.FindFirst("uid")?.Value;
            var author = await _context.Users.FirstOrDefaultAsync(u => u.Uid == authorId);
            var user = await _context.Users.Include(u => u.UidsNavigation).FirstOrDefaultAsync(u => u.Uid == uid);

            if (author == null || user == null)
                return NotFound();
            user.UidsNavigation.Remove(author);
            await _context.SaveChangesAsync();
            return Ok();

        }
        // Bỏ chặn tác giả
        [Authorize]
        [HttpPost("UnBlockAuthor")]
        public IActionResult UnBlockAuthor([FromForm] string authorId)
        {
            var uid = User.FindFirst("uid")?.Value;

            var user = _context.Users
                .Include(x => x.Authors)
                .FirstOrDefault(x => x.Uid == uid);

            if (user == null)
                return NotFound();

            var author = user.Authors
                .FirstOrDefault(x => x.Uid == authorId);

            if (author != null)
            {
                user.Authors.Remove(author);
                _context.SaveChanges();
            }

            return Ok();
        }
        [Authorize]
        [HttpPost("BlockAuthor")]
        public IActionResult BlockAuthor([FromForm] string authorId)
        {
            var uid = User.FindFirst("uid")?.Value;

            var user = _context.Users
                .Include(x => x.Authors)
                .FirstOrDefault(x => x.Uid == uid);

            var author = _context.Users
                .FirstOrDefault(x => x.Uid == authorId);

            if (user == null || author == null)
                return NotFound();

            if (!user.Authors.Any(x => x.Uid == authorId))
            {
                user.Authors.Add(author);
                _context.SaveChanges();
            }

            return Ok();
        }

        [Authorize]
        [HttpGet("followedNovels")]
        public async Task<IActionResult> GetFollowedNovels()
        {
            var uid = User.FindFirst("uid")?.Value;
            var user = await _context.Users
                .Include(x => x.Novels)
                    .ThenInclude(n => n.UidNavigation)
                .FirstOrDefaultAsync(x => x.Uid == uid);

            if (user == null)
                return NotFound();

            var result = user.Novels.Select(n => new
            {
                novelId = n.NovelId,
                title = n.Title,
                avartarNovelUrl = n.ImageNovelUrl,
                authorName = n.UidNavigation != null ? n.UidNavigation.DisplayName : "",
                description = n.Description
            }).ToList();

            return Ok(result);
        }

        [Authorize]
        [HttpGet("followedAuthors")]
        public async Task<IActionResult> GetFollowedAuthors()
        {
            var uid = User.FindFirst("uid")?.Value;
            var user = await _context.Users
                .Include(x => x.UidsNavigation)
                .FirstOrDefaultAsync(x => x.Uid == uid);

            if (user == null)
                return NotFound();

            var result = user.UidsNavigation.Select(a => new
            {
                uid = a.Uid,
                displayName = a.DisplayName,
                avartarUrl = a.AvartarUrl
            }).ToList();

            return Ok(result);
        }

        [Authorize]
        [HttpGet("blockedAuthors")]
        public async Task<IActionResult> GetBlockedAuthors()
        {
            var uid = User.FindFirst("uid")?.Value;
            var user = await _context.Users
                .Include(x => x.Authors)
                .FirstOrDefaultAsync(x => x.Uid == uid);

            if (user == null)
                return NotFound();

            var result = user.Authors.Select(a => new
            {
                uid = a.Uid,
                displayName = a.DisplayName,
                avartarUrl = a.AvartarUrl
            }).ToList();

            return Ok(result);
        }
    }
}
