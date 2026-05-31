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
//                       _oo0oo_
//                      o8888888o
//                      88" . "88
//                      (| -_- |)
//                      0\  =  /0
//                    ___/`---'\___
//                  .' \\|     |// '.
//                 / \\|||  :  |||// \
//                / _||||| -:- |||||- \
//               |   | \\\  -  /// |   |
//               | \_|  ''\---/''  |_/ |
//               \  .-\__  '-'  ___/-. /
//             ___'. .'  /--.--\  `. .'___
//          ."" '<  `.___\_<|>_/___.' >' "".
//         | | :  `- \`.;`\ _ /`;.`/ - ` : | |
//         \  \ `_.   \_ __\ /__ _/   .-` /  /
//     =====`-.____`.___ \_____/___.-`___.-'=====
//                       `=---='
//
//     ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//            Phật phù hộ, không bao giờ BUG
//     ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
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
        private readonly FileStorageServices _fileServicesUpload;
        public NovelController(JwtServices jwtServices, SmartTruyenDbContext context, 
            MailServices mailServices, IMemoryCache cache, FileStorageServices fileServicesUpload)
        {
            _jwtServices = jwtServices;
            _context = context;
            _mailServices = mailServices;
            _cache = cache;
            _fileServicesUpload = fileServicesUpload;
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
        [Authorize]
        [HttpPost("createNovel")]
        public async Task<IActionResult> createNovel(UserRequests.CreateNovelRequest req)
        {
            var uid = User.FindFirst("uid")?.Value;
            if (uid == null)
                return Unauthorized();
            var newNovel = new Novel();
            Guid idNovel = Guid.NewGuid();
            newNovel.NovelId = idNovel.ToString();
            newNovel.Status = req.Status;
            newNovel.Title = req.Title;

            string slug = req.Title.ToLowerInvariant();
            // slug xử lý sau,dùng GUID mẹ đi, thuật toán nó đau đầu vcl
            newNovel.Slug = idNovel.ToString();
            newNovel.Description = req.Description;
            newNovel.UpdateTime = DateTime.Now;
            newNovel.CreateTime = DateTime.Now;
            newNovel.AgeRating = req.AgeRating;
            // từ từ up file
            newNovel.ImageNovelUrl = "";
            newNovel.ImageBanerNovelUrl = "";
            newNovel.Uid = uid;

            try
            {
                _context.Novels.Add(newNovel);
                await _context.SaveChangesAsync();
                var novelTemp = await _context.Novels
                .Include(n => n.Categories)
                .FirstOrDefaultAsync(n => n.NovelId == idNovel.ToString());
                foreach (var categoryId in req.Genres)
                {


                    var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

                    if (novelTemp != null && category != null)
                    {
                        novelTemp.Categories.Add(category);
                        
                    }
                }
                await _context.SaveChangesAsync();
                string publicLink = "https://pub-20056e4912f440f08b3d40eea545f95f.r2.dev/smart-novel/novel-image/";
                Guid idFile = Guid.NewGuid();
                Guid idFile1 = Guid.NewGuid();
                var novel1 = await _context.Novels.FirstOrDefaultAsync(n => n.NovelId == idNovel.ToString());
                if (req.CoverImage != null)
                {
                    string fileCoverNameRaw = req.CoverImage.FileName;
                    string fileCoverName = $"{idFile.ToString()}-{fileCoverNameRaw}";
                    var resultUploadCover = await _fileServicesUpload.UploadFile("smart-novel/novel-image/",
                        fileCoverName, req.CoverImage);
                    if (resultUploadCover)
                    {

                        novel1.ImageNovelUrl = publicLink+ fileCoverName;

                    }
                }
                if(req.BannerImage != null)
                {
                    string fileBannerNameRaw = req.BannerImage.FileName;

                    string fileBannerName = $"{idFile1.ToString()}-{fileBannerNameRaw}";

                    var resultUploadBanner = await _fileServicesUpload.UploadFile("smart-novel/novel-image/",
                        fileBannerName, req.CoverImage);


                    if (resultUploadBanner)
                    {
                        novel1.ImageBanerNovelUrl = publicLink+fileBannerName;
                    }
                }

                _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return BadRequest(new
                {
                    Msg = "Something went wrong huhuhuuhhu"
                });
            }

        }
    }
}
