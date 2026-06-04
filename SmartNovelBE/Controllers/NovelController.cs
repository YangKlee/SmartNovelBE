//                    _oo0oo_
//                   o8888888o
//                   88" . "88
//                   (| -_- |)
//                   0\  =  /0
//                 ___/`---'\___
//               .' \\|     |// '.
//              / \\|||  :  |||// \
//             / _||||| -:- |||||- \
//            |   | \\\  -  /// |   |
//            | \_|  ''\---/''  |_/ |
//            \  .-\__  '-'  ___/-. /
//          ___'. .'  /--.--\  `. .'___
//       ."" '<  `.___\_<|>_/___.' >' "".
//      | | :  `- \`.;`\ _ /`;.`/ - ` : | |
//      \  \ `_.   \_ __\ /__ _/   .-` /  /
//  =====`-.____`.___ \_____/___.-`___.-'=====
//                    `=---='
//
//  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//        Phật phù hộ, không bao giờ BUG
//  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
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
using SSmartNovelBE.Services.Interfaces;
using System.Security.Claims;
using System.Security.Cryptography;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace SmartNovelBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NovelController : ControllerBase
    {
        private readonly INovelService _novelService;
        private readonly JwtServices _jwtServices;
        private readonly SmartTruyenDbContext _context;
        private readonly MailServices _mailServices;
        private readonly IMemoryCache _cache;
        private readonly FileStorageServices _fileServicesUpload;

        public NovelController(
            INovelService novelService,
            JwtServices jwtServices,
            SmartTruyenDbContext context,
            MailServices mailServices,
            IMemoryCache cache,
            FileStorageServices fileServicesUpload)
        {
            _novelService = novelService;
            _jwtServices = jwtServices;
            _context = context;
            _mailServices = mailServices;
            _cache = cache;
            _fileServicesUpload = fileServicesUpload;
        }

        [HttpGet("{novelID}")]
        public async Task<IActionResult> GetDetail(string novelID)
        {
            var result = await _context.Novels.FirstOrDefaultAsync(n => n.NovelId == novelID);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("{novelId}/chapters")]
        public async Task<IActionResult> GetChapters(string novelId)
        {
            var result = await _context.Chapters.Where(c => c.NovelId == novelId && c.Status.ToLower() == "public").ToListAsync();
            return Ok(result);
        }

        [HttpGet("getUserNovel")]
        [Authorize]
        public async Task<IActionResult> getListUserNovel([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10000000)
        {
            var uid = User.FindFirst("uid")?.Value;
            if (uid == null)
                return Unauthorized();

            var query = _context.Novels.Where(n => n.Uid == uid);

            var totalRecords = await query.CountAsync();
            int TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            var novels = await query
                .Select(n => new UserRespone.NovelResponseAuthor2
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
                    categories = n.Categories,
                    countChapter = n.Chapters.Count(),
                    countChapterPublic = n.Chapters.Count(c => c.Status == "Public"),
                    countChapterDraf = n.Chapters.Count(c => c.Status == "Draft"),
                    countChapterRemove = n.Chapters.Count(c => c.Status == "Cancel"),
                    novelRating = n.Ratings
                        .Select(r => (double?)r.RatingPoint)
                        .Average() ?? 0
                })
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            //var res = new PaginationRespone<UserRespone.NovelResponseAuthor2>
            //{
            //    Data = novels,
            //    TotalRecords = totalRecords,
            //    PageNumber = pageNumber,
            //    PageSize = pageSize,
            //};

            // Đã sửa: Trả về res thay vì novels

            return Ok(novels);
        }
        [HttpGet("getUserNovel/count")]
        [Authorize]
        public async Task<IActionResult> getUserNovelCount()
        {
            var uid = User.FindFirst("uid")?.Value;
            if (uid == null) return Unauthorized();

            var totalRecords = await _context.Novels.CountAsync(n => n.Uid == uid);

            return Ok(totalRecords);
        }
        [HttpGet("getInfoNovelForReader/{id}")]
        public async Task<IActionResult> getInfoNovelForReader(string id)
        {
            var roleId = User.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrEmpty(roleId) || roleId == "4")
            {
                var checkStatusNovel = await _context.Novels
                    .AnyAsync(n => n.NovelId == id && n.Status != null && n.Status != "public");

                if (checkStatusNovel)
                    return Unauthorized();
            }

            var novels = await _context.Novels
                .Where(n => n.NovelId == id)
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
                    authorId = n.Uid,
                    authorName = n.UidNavigation.DisplayName,
                    categories = n.Categories,
                    novelRating = n.Ratings
                    .Select(r => (double?)r.RatingPoint)
                    .Average() ?? 0
                })
                .FirstOrDefaultAsync();

            if (novels == null)
            {
                return BadRequest(new
                {
                    msg = "Không tìm thấy truyện",
                });
            }

            return Ok(novels);
        }

        [Authorize]
        [HttpPost("createNovel")]
        public async Task<IActionResult> createNovel(UserRequests.CreateNovelRequest req)
        {
            var uid = User.FindFirst("uid")?.Value;
            if (uid == null)
                return Unauthorized();
            if (req.Status != "Public" && req.Status != "Draft")
                return BadRequest();
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
            foreach (var categoryId in req.Genres)
            {

                var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
                if (newNovel != null && category != null)
                {
                    newNovel.Categories.Add(category);
                }
            }
            try
            {
                _context.Novels.Add(newNovel);
                await _context.SaveChangesAsync();

                var novelTemp = await _context.Novels
                .Include(n => n.Categories)
                .FirstOrDefaultAsync(n => n.NovelId == idNovel.ToString());



                //await _context.SaveChangesAsync();

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
                        novel1.ImageNovelUrl = publicLink + fileCoverName;
                    }
                }

                if (req.BannerImage != null)
                {
                    string fileBannerNameRaw = req.BannerImage.FileName;
                    string fileBannerName = $"{idFile1.ToString()}-{fileBannerNameRaw}";

                    var resultUploadBanner = await _fileServicesUpload.UploadFile("smart-novel/novel-image/",
                        fileBannerName, req.BannerImage);
                    if (resultUploadBanner)
                    {
                        novel1.ImageBanerNovelUrl = publicLink + fileBannerName;
                    }
                }

                await _context.SaveChangesAsync();
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
        [Authorize]
        [HttpPut("modifyNovel/{novelID}")]
        public async Task<IActionResult> updateNovel(string novelID, UserRequests.ModifyNovelRequest req)
        {
            var uid = User.FindFirst("uid")?.Value;
            if (uid == null)
                return Unauthorized();
            if (req.Status != "Public" && req.Status != "Draft")
                return BadRequest();
            // tránh mấy thằng tày lấy id truyện và gửi request update
            var modifyNovel = await _context.Novels.Include(x => x.Categories).FirstOrDefaultAsync(x => x.NovelId == novelID && x.Uid == uid);
            if (modifyNovel == null)
                return BadRequest(new { Msg = "Trứng mà đòi khôn hơn vịt" });
            modifyNovel.Title = req.Title;
            modifyNovel.Description = req.Description;
            modifyNovel.UpdateTime = DateTime.Now;
            modifyNovel.Status = req.Status;
            modifyNovel.AgeRating = req.AgeRating;
            modifyNovel.Categories.Clear();
            foreach (var categoryId in req.Genres)
            {

                var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
                if (modifyNovel != null && category != null)
                {
                    modifyNovel.Categories.Add(category);
                }
            }
            //try
            {

                await _context.SaveChangesAsync();



                //await _context.SaveChangesAsync();

                string publicLink = "https://pub-20056e4912f440f08b3d40eea545f95f.r2.dev/smart-novel/novel-image/";
                Guid idFile = Guid.NewGuid();
                Guid idFile1 = Guid.NewGuid();

                //string prefix = "https://pub-20056e4912f440f08b3d40eea545f95f.r2.dev/smart-novel/novel-image/";
                var novel1 = await _context.Novels.FirstOrDefaultAsync(n => n.NovelId == novelID);

                if (req.CoverImage != null)
                {
                    // xóa file cũ
                    if (novel1.ImageNovelUrl != null)
                    {
                        var fileOldName = novel1.ImageNovelUrl.Replace(publicLink, "");
                        await _fileServicesUpload.DeleteFile("smart-novel/novel-image/", fileOldName);
                    }

                    string fileCoverNameRaw = req.CoverImage.FileName;
                    string fileCoverName = $"{idFile.ToString()}-{fileCoverNameRaw}";
                    var resultUploadCover = await _fileServicesUpload.UploadFile("smart-novel/novel-image/",
                        fileCoverName, req.CoverImage);

                    if (resultUploadCover)
                    {
                        novel1.ImageNovelUrl = publicLink + fileCoverName;
                    }
                }

                if (req.BannerImage != null)
                {
                    // xóa file cũ
                    if (novel1.ImageBanerNovelUrl != null)
                    {
                        var fileOldName = novel1.ImageBanerNovelUrl.Replace(publicLink, "");
                        await _fileServicesUpload.DeleteFile("smart-novel/novel-image/", fileOldName);
                    }
                    string fileBannerNameRaw = req.BannerImage.FileName;
                    string fileBannerName = $"{idFile1.ToString()}-{fileBannerNameRaw}";

                    var resultUploadBanner = await _fileServicesUpload.UploadFile("smart-novel/novel-image/",
                        fileBannerName, req.BannerImage);

                    if (resultUploadBanner)
                    {
                        novel1.ImageBanerNovelUrl = publicLink + fileBannerName;
                    }
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            //catch
            {
                return BadRequest(new
                {
                    Msg = "Something went wrong huhuhuuhhu"
                });
            }
        }
        [Authorize]
        [HttpDelete("deleteNovel/{NovelID}")]
        public async Task<IActionResult> deleteNovel(string NovelID)
        {
            var uid = User.FindFirst("uid")?.Value;
            if (uid == null)
                return Unauthorized();
            // tránh mấy thằng tày lấy id truyện và gửi request update
            var deleteNovel = await _context.Novels.Include(x => x.Chapters).FirstOrDefaultAsync(x => x.NovelId == NovelID && x.Uid == uid);
            if (deleteNovel == null)
                return BadRequest(new { Msg = "Trứng mà đòi khôn hơn vịt" });

            // gỡ file ảnh khỏi cloud trước
            string publicLinkImage = "https://pub-20056e4912f440f08b3d40eea545f95f.r2.dev/smart-novel/novel-image/";
            string publicChapterFile = "https://pub-20056e4912f440f08b3d40eea545f95f.r2.dev/smart-novel/novel-file/";
            try
            {
                if (deleteNovel.ImageNovelUrl != null)
                {
                    string fileOldName = deleteNovel.ImageNovelUrl.Replace(publicLinkImage, "");
                    await _fileServicesUpload.DeleteFile("smart-novel/novel-image/", fileOldName);
                }
                if (deleteNovel.ImageBanerNovelUrl != null)
                {
                    string fileOldName = deleteNovel.ImageBanerNovelUrl.Replace(publicLinkImage, "");
                    await _fileServicesUpload.DeleteFile("smart-novel/novel-image/", fileOldName);
                }

                // quét hết các chapter, gỡ toàn bộ file chapter
                foreach (Chapter c in deleteNovel.Chapters)
                {
                    if (c.ChapterFileUrl != null)
                    {
                        string fileOldName = c.ChapterFileUrl.Replace(publicChapterFile, "");
                        await _fileServicesUpload.DeleteFile("smart-novel/novel-file/", fileOldName);

                    }
                    //_context.Chapters.Remove(c);
                }
                _context.Novels.Remove(deleteNovel);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return BadRequest();
            }

        }
        [Authorize]
        [HttpGet("seachNovelAuthor")]
        public async Task<IActionResult> seachNovel([FromQuery] UserRequests.searchNovel req, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 1000000)
        {
            var uid = User.FindFirst("uid")?.Value;

            if (req.status.ToLower() == "all")
            {
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
                       categories = n.Categories,
                       countChapter = n.Chapters.Count(),
                       countChapterPublic = n.Chapters.Count(c => c.Status == "Public"),
                       countChapterDraf = n.Chapters.Count(c => c.Status == "Draft"),
                       countChapterRemove = n.Chapters.Count(c => c.Status == "Cancel"),
                       novelRating = n.Ratings
                       .Select(r => (double?)r.RatingPoint)
                       .Average() ?? 0
                   }).ToListAsync();
                if (req.keyworld != null)
                {
                    var res = novels.Where(n => n.Title.Contains(req.keyworld)).ToList();
                    return Ok(res);
                }
                return Ok(novels);
            }
            else
            {
                var type = req.status.ToLower();
                var novels = await _context.Novels
                   .Where(n => n.Uid == uid && n.Status.ToLower() == type)
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
                       categories = n.Categories,
                       countChapter = n.Chapters.Count(),
                       countChapterPublic = n.Chapters.Count(c => c.Status == "Public"),
                       countChapterDraf = n.Chapters.Count(c => c.Status == "Draft"),
                       countChapterRemove = n.Chapters.Count(c => c.Status == "Cancel"),
                       novelRating = n.Ratings
                       .Select(r => (double?)r.RatingPoint)
                       .Average() ?? 0
                   }).ToListAsync();
                if (req.keyworld != null)
                {
                    var res = novels.Where(n => n.Title.Contains(req.keyworld)).ToList();
                    return Ok(res);
                }
                return Ok(novels);
            }



        }
        [Authorize]
        [HttpGet("seachNovelAuthor/count")]
        public async Task<IActionResult> countseachNovel([FromQuery] UserRequests.searchNovel req)
        {
            var uid = User.FindFirst("uid")?.Value;

            if (req.status.ToLower() == "all")
            {
                var novels = await _context.Novels
                   .Where(n => n.Uid == uid).ToListAsync();
                if (req.keyworld != null)
                {
                    var res = novels.Where(n => n.Title.Contains(req.keyworld)).ToList();
                    return Ok(res.Count);
                }
                return Ok(novels.Count);
            }
            else
            {
                var type = req.status.ToLower();
                var novels = await _context.Novels
                   .Where(n => n.Uid == uid && n.Status.ToLower() == type)
                 .ToListAsync();
                if (req.keyworld != null)
                {
                    var res = novels.Where(n => n.Title.Contains(req.keyworld)).ToList();
                    return Ok(res.Count);
                }
                return Ok(novels.Count);
            }
        }
    }
}