using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SmartNovelBE.Models;
using SmartNovelBE.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartNovelBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChaptersController : ControllerBase
    {
        private readonly JwtServices _jwtServices;
        private readonly SmartTruyenDbContext _context;
        private readonly MailServices _mailServices;
        private readonly IMemoryCache _cache;
        private readonly FileStorageServices _fileServicesUpload;
        public ChaptersController(JwtServices jwtServices, SmartTruyenDbContext context,
            MailServices mailServices, IMemoryCache cache, FileStorageServices fileServicesUpload)
        {
            _jwtServices = jwtServices;
            _context = context;
            _mailServices = mailServices;
            _cache = cache;
            _fileServicesUpload = fileServicesUpload;
        }
        // GET: api/Chapters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Chapter>>> GetChapters()
        {
            return await _context.Chapters.ToListAsync();
        }

        // GET: api/Chapters/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Chapter>> GetChapter(string id)
        {
            var chapter = await _context.Chapters.FindAsync(id);

            if (chapter == null)
            {
                return NotFound();
            }

            return chapter;
        }

        // PUT: api/Chapters/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutChapter(string id, Chapter chapter)
        {
            if (id != chapter.ChapterId)
            {
                return BadRequest();
            }

            _context.Entry(chapter).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChapterExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Chapters
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Chapter>> PostChapter(Chapter chapter)
        {
            _context.Chapters.Add(chapter);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ChapterExists(chapter.ChapterId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetChapter", new { id = chapter.ChapterId }, chapter);
        }

        // DELETE: api/Chapters/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChapter(string id)
        {
            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null)
            {
                return NotFound();
            }

            _context.Chapters.Remove(chapter);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ChapterExists(string id)
        {
            return _context.Chapters.Any(e => e.ChapterId == id);
        }
        [HttpGet("getChapterByNovel/{novelID}")]
        public async Task<ActionResult<Chapter>> getChapterByNovel(string novelID)
        {
            // rào trước đề phòng lấy id và xem truyện người khác
            var uid = User.FindFirst("uid")?.Value;
            var ehe = await _context.Novels.AnyAsync(n => n.NovelId == novelID && n.Uid == uid);
            if (!ehe)
                return Unauthorized();
            var chapters=  await _context.Chapters.Where(c => c.NovelId == novelID).OrderBy(c => c.ChaperOrder).ToListAsync();
            return Ok(chapters);
        }
        [Authorize]
        [HttpPost("createChapter/{novelID}")]
        public async Task<IActionResult> createChapter(string novelID,UserRequests.chapterCreate req)
        {
            // rào trước đề phòng lấy id và đăng chương trong truyện người khác
            var uid = User.FindFirst("uid")?.Value;
            var ehe = await _context.Novels.AnyAsync(n => n.NovelId == novelID && n.Uid == uid);
            if (!ehe)
                return BadRequest(new
                {
                    Msg = "Cưng tính copy id truyện người ta rồi gửi request đăng chương à, đâu có dễ dị"
                });
            var newChapter = new Chapter();
            string idChapter = Guid.NewGuid().ToString();
            newChapter.ChapterTitle = req.title;
            newChapter.ChapterId = idChapter;
            newChapter.SummaryChapter = req.decrip;
            newChapter.AllowComment = req.allowComment;
            newChapter.Status = req.status;
            newChapter.UpdateTime = DateTime.Now;
            newChapter.CreateTime = DateTime.Now;
            newChapter.NovelId = novelID;
            try
            {
                // upload file
                string publicLink = "https://pub-20056e4912f440f08b3d40eea545f95f.r2.dev/smart-novel/novel-file/";

                string fileName = $"{Guid.NewGuid().ToString()}-{idChapter}.html";
                newChapter.ChapterFileUrl = publicLink + fileName;
                var res = await _fileServicesUpload.UploadHtml("smart-novel/novel-file/", fileName, req.content);
                if (res)
                    newChapter.ChapterFileUrl = publicLink + fileName;
               
                _context.Chapters.Add(newChapter);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return BadRequest();
            }

        }


    }
}
