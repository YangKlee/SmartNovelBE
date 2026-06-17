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
        // DANH SÁCH TRUYỆN BỊ BÁO CÁO
        // =====================================================
        [HttpGet("reported-novels")]
        public async Task<IActionResult> GetReportedNovels()
        {
            var data = await (
                from rt in _context.ReportTickets
                join n in _context.Novels
                    on rt.NovelId equals n.NovelId
                where rt.Status == "PENDING"
                   && rt.Type == "NOVEL"
                select new
                {
                    rt.TiketId,
                    rt.ReasonDetail,
                    rt.TimeSend,

                    n.NovelId,
                    n.Title,
                    n.Description,
                    n.ImageNovelUrl,
                    n.Status,
                    n.Uid
                })
                .OrderByDescending(x => x.TimeSend)
                .ToListAsync();

            return Ok(data);
        }

        // =====================================================
        // DANH SÁCH CHƯƠNG BỊ BÁO CÁO
        // =====================================================
        [HttpGet("reported-chapters")]
        public async Task<IActionResult> GetReportedChapters()
        {
            var data = await (
                from rt in _context.ReportTickets
                join c in _context.Chapters
                    on rt.ChapterId equals c.ChapterId
                join n in _context.Novels
                    on c.NovelId equals n.NovelId
                where rt.Status == "PENDING"
                   && rt.Type == "CHAPTER"
                select new
                {
                    rt.TiketId,
                    rt.ReasonDetail,
                    rt.TimeSend,

                    c.ChapterId,
                    c.ChapterTitle,
                    c.SummaryChapter,
                    c.ChapterFileUrl,
                    c.Status,

                    n.NovelId,
                    n.Title
                })
                .OrderByDescending(x => x.TimeSend)
                .ToListAsync();

            return Ok(data);
        }

        // =====================================================
        // DANH SÁCH BÌNH LUẬN BỊ BÁO CÁO
        // =====================================================
        [HttpGet("reported-comments")]
        public async Task<IActionResult> GetReportedComments()
        {
            var data = await (
                from rt in _context.ReportTickets
                join cm in _context.Comments
                    on rt.CommentId equals cm.CommentId
                join ch in _context.Chapters
                    on cm.ChapterId equals ch.ChapterId
                join nv in _context.Novels
                    on ch.NovelId equals nv.NovelId
                where rt.Status == "PENDING"
                   && rt.Type == "COMMENT"
                select new
                {
                    rt.TiketId,
                    rt.ReasonDetail,
                    rt.TimeSend,

                    cm.CommentId,
                    cm.Content,
                    cm.Status,
                    cm.Uid,

                    ch.ChapterId,
                    ch.ChapterTitle,

                    nv.NovelId,
                    nv.Title
                })
                .OrderByDescending(x => x.TimeSend)
                .ToListAsync();

            return Ok(data);
        }

        // =====================================================
        // GỠ TRUYỆN VI PHẠM
        // =====================================================
        [HttpPut("remove-novel/{ticketId}")]
        public async Task<IActionResult> RemoveNovel(
            string ticketId,
            [FromQuery] string moderatorUid)
        {
            var ticket = await _context.ReportTickets
                .FirstOrDefaultAsync(x =>
                    x.TiketId == ticketId &&
                    x.Type == "NOVEL" &&
                    x.Status == "PENDING");

            if (ticket == null)
                return NotFound("Ticket not found");

            var novel = await _context.Novels
                .FirstOrDefaultAsync(x =>
                    x.NovelId == ticket.NovelId);

            if (novel == null)
                return NotFound("Novel not found");

            novel.Status = "REMOVED";

            ticket.Status = "RESOLVED";
            ticket.ResolvedUid = moderatorUid;

            await _context.SaveChangesAsync();

            return Ok("Novel removed successfully");
        }

        // =====================================================
        // GỠ CHƯƠNG VI PHẠM
        // =====================================================
        [HttpPut("remove-chapter/{ticketId}")]
        public async Task<IActionResult> RemoveChapter(
            string ticketId,
            [FromQuery] string moderatorUid)
        {
            var ticket = await _context.ReportTickets
                .FirstOrDefaultAsync(x =>
                    x.TiketId == ticketId &&
                    x.Type == "CHAPTER" &&
                    x.Status == "PENDING");

            if (ticket == null)
                return NotFound("Ticket not found");

            var chapter = await _context.Chapters
                .FirstOrDefaultAsync(x =>
                    x.ChapterId == ticket.ChapterId);

            if (chapter == null)
                return NotFound("Chapter not found");

            chapter.Status = "REMOVED";

            ticket.Status = "RESOLVED";
            ticket.ResolvedUid = moderatorUid;

            await _context.SaveChangesAsync();

            return Ok("Chapter removed successfully");
        }

        // =====================================================
        // GỠ BÌNH LUẬN VI PHẠM
        // =====================================================
        [HttpPut("remove-comment/{ticketId}")]
        public async Task<IActionResult> RemoveComment(
            string ticketId,
            [FromQuery] string moderatorUid)
        {
            var ticket = await _context.ReportTickets
                .FirstOrDefaultAsync(x =>
                    x.TiketId == ticketId &&
                    x.Type == "COMMENT" &&
                    x.Status == "PENDING");

            if (ticket == null)
                return NotFound("Ticket not found");

            var comment = await _context.Comments
                .FirstOrDefaultAsync(x =>
                    x.CommentId == ticket.CommentId);

            if (comment == null)
                return NotFound("Comment not found");

            comment.Status = "REMOVED";

            ticket.Status = "RESOLVED";
            ticket.ResolvedUid = moderatorUid;

            await _context.SaveChangesAsync();

            return Ok("Comment removed successfully");
        }

        // =====================================================
        // BÁC BỎ BÁO CÁO
        // =====================================================
        [HttpPut("reject-ticket/{ticketId}")]
        public async Task<IActionResult> RejectTicket(
            string ticketId,
            [FromQuery] string moderatorUid)
        {
            var ticket = await _context.ReportTickets
                .FirstOrDefaultAsync(x =>
                    x.TiketId == ticketId &&
                    x.Status == "PENDING");

            if (ticket == null)
                return NotFound("Ticket not found");

            ticket.Status = "REJECTED";
            ticket.ResolvedUid = moderatorUid;

            await _context.SaveChangesAsync();

            return Ok("Report rejected");
        }

        // =====================================================
        // LỊCH SỬ XỬ LÝ BÁO CÁO
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

                    x.NovelId,
                    x.ChapterId,
                    x.CommentId,

                    x.RepoterUid,
                    x.ResolvedUid,

                    x.Status,
                    x.TimeSend
                })
                .OrderByDescending(x => x.TimeSend)
                .ToListAsync();

            return Ok(data);
        }
    }


}


