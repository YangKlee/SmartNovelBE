using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartNovelBE.Models;

namespace SmartNovelBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModerationController : ControllerBase
    {
        private readonly SmartTruyenDbContext _context;


    public ModerationController(SmartTruyenDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // DANH SÁCH TRUYỆN CHỜ DUYỆT
        // =====================================================
        [HttpGet("novels-pending")]
        public async Task<IActionResult> GetPendingNovels()
        {
            var data = await _context.Novels
                .Where(x => x.Status == "PENDING")
                .Select(x => new
                {
                    x.NovelId,
                    x.Title,
                    x.Uid,
                    x.CreateTime,
                    x.UpdateTime
                })
                .OrderByDescending(x => x.CreateTime)
                .ToListAsync();

            return Ok(data);
        }

        // =====================================================
        // PHÊ DUYỆT TRUYỆN
        [HttpPut("approve-novel/{novelId}")]
        public async Task<IActionResult> ApproveNovel(string novelId)
        {
            var novel = await _context.Novels
                .FirstOrDefaultAsync(x => x.NovelId == novelId);

            if (novel == null)
                return NotFound("Novel not found");

            novel.Status = "ACTIVE";

            await _context.SaveChangesAsync();

            return Ok("Novel approved");
        }

        // =====================================================
        // TỪ CHỐI TRUYỆN
        // =====================================================
        [HttpPut("reject-novel/{novelId}")]
        public async Task<IActionResult> RejectNovel(string novelId)
        {
            var novel = await _context.Novels
                .FirstOrDefaultAsync(x => x.NovelId == novelId);

            if (novel == null)
                return NotFound("Novel not found");

            novel.Status = "REJECTED";

            await _context.SaveChangesAsync();

            return Ok("Novel rejected");
        }

        // =====================================================
        // DANH SÁCH CHƯƠNG CHỜ DUYỆT
        // =====================================================
        [HttpGet("chapters-pending")]
        public async Task<IActionResult> GetPendingChapters()
        {
            var data = await _context.Chapters
                .Where(x => x.Status == "PENDING")
                .Select(x => new
                {
                    x.ChapterId,
                    x.ChapterTitle,
                    x.NovelId,
                    x.CreateTime,
                    x.UpdateTime
                })
                .OrderByDescending(x => x.CreateTime)
                .ToListAsync();

            return Ok(data);
        }

        // =====================================================
        // PHÊ DUYỆT CHƯƠNG
        // =====================================================
        [HttpPut("approve-chapter/{chapterId}")]
        public async Task<IActionResult> ApproveChapter(string chapterId)
        {
            var chapter = await _context.Chapters
                .FirstOrDefaultAsync(x => x.ChapterId == chapterId);

            if (chapter == null)
                return NotFound("Chapter not found");

            chapter.Status = "ACTIVE";

            await _context.SaveChangesAsync();

            return Ok("Chapter approved");
        }

        // =====================================================
        // TỪ CHỐI CHƯƠNG
        // =====================================================
        [HttpPut("reject-chapter/{chapterId}")]
        public async Task<IActionResult> RejectChapter(string chapterId)
        {
            var chapter = await _context.Chapters
                .FirstOrDefaultAsync(x => x.ChapterId == chapterId);

            if (chapter == null)
                return NotFound("Chapter not found");

            chapter.Status = "REJECTED";

            await _context.SaveChangesAsync();

            return Ok("Chapter rejected");
        }

        // =====================================================
        // DANH SÁCH TICKET CHỜ XỬ LÝ
        // =====================================================
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingTickets()
        {
            var data = await _context.ReportTickets
                .Where(x => x.Status == "PENDING")
                .Select(x => new
                {
                    x.TiketId,
                    x.Type,
                    x.ReasonDetail,
                    x.NovelId,
                    x.ChapterId,
                    x.CommentId,
                    x.TargetUid,
                    x.RepoterUid,
                    x.TimeSend
                })
                .OrderByDescending(x => x.TimeSend)
                .ToListAsync();

            return Ok(data);
        }

        // =====================================================
        // LỊCH SỬ KIỂM DUYỆT
        // =====================================================
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var data = await _context.ReportTickets
                .Where(x =>
                    x.Status == "RESOLVED" ||
                    x.Status == "REJECTED")
                .Select(x => new
                {
                    x.TiketId,
                    x.Type,
                    x.ReasonDetail,
                    x.RepoterUid,
                    x.ResolvedUid,
                    x.Status,
                    x.TimeSend
                })
                .OrderByDescending(x => x.TimeSend)
                .ToListAsync();

            return Ok(data);
        }

        // =====================================================
        // PHÊ DUYỆT TICKET
        // =====================================================
        [HttpPut("approve/{ticketId}")]
        public async Task<IActionResult> ApproveTicket(
            string ticketId,
            [FromQuery] string moderatorUid)
        {
            var ticket = await _context.ReportTickets
                .FirstOrDefaultAsync(x => x.TiketId == ticketId);

            if (ticket == null)
                return NotFound("Ticket not found");

            ticket.Status = "RESOLVED";
            ticket.ResolvedUid = moderatorUid;

            await _context.SaveChangesAsync();

            return Ok("Approved");
        }

        // =====================================================
        // TỪ CHỐI TICKET
        // =====================================================
        [HttpPut("reject/{ticketId}")]
        public async Task<IActionResult> RejectTicket(
            string ticketId,
            [FromQuery] string moderatorUid)
        {
            var ticket = await _context.ReportTickets
                .FirstOrDefaultAsync(x => x.TiketId == ticketId);

            if (ticket == null)
                return NotFound("Ticket not found");

            ticket.Status = "REJECTED";
            ticket.ResolvedUid = moderatorUid;

            await _context.SaveChangesAsync();

            return Ok("Rejected");
        }

        // =====================================================
        // GỠ TRUYỆN VI PHẠM
        // =====================================================
        [HttpPut("remove-novel/{novelId}")]
        public async Task<IActionResult> RemoveNovel(string novelId)
        {
            var novel = await _context.Novels
                .FirstOrDefaultAsync(x => x.NovelId == novelId);

            if (novel == null)
                return NotFound("Novel not found");

            novel.Status = "REMOVED";

            await _context.SaveChangesAsync();

            return Ok("Novel removed");
        }

        // =====================================================
        // GỠ CHƯƠNG VI PHẠM
        // =====================================================
        [HttpPut("remove-chapter/{chapterId}")]
        public async Task<IActionResult> RemoveChapter(string chapterId)
        {
            var chapter = await _context.Chapters
                .FirstOrDefaultAsync(x => x.ChapterId == chapterId);

            if (chapter == null)
                return NotFound("Chapter not found");

            chapter.Status = "REMOVED";

            await _context.SaveChangesAsync();

            return Ok("Chapter removed");
        }

        // =====================================================
        // GỠ COMMENT VI PHẠM
        // =====================================================
        [HttpPut("remove-comment/{commentId}")]
        public async Task<IActionResult> RemoveComment(string commentId)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(x => x.CommentId == commentId);

            if (comment == null)
                return NotFound("Comment not found");

            comment.Status = "REMOVED";

            await _context.SaveChangesAsync();

            return Ok("Comment removed");
        }
    }


}
