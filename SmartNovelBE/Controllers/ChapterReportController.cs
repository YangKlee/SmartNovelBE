using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartNovelBE.Models;
using SmartNovelBE.Models.DTOs;

namespace SmartNovelBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChapterReportController : ControllerBase
    {
        private readonly SmartTruyenDbContext _context;

        public ChapterReportController(
            SmartTruyenDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport(
            ChapterReportDto model)
        {
            var chapter = await _context.Chapters
                .FirstOrDefaultAsync(x =>
                    x.ChapterId == model.ChapterId);

            if (chapter == null)
            {
                return NotFound("Chapter not found");
            }

            var novel = await _context.Novels
                .FirstOrDefaultAsync(x =>
                    x.NovelId == chapter.NovelId);

            var ticket = new ReportTicket
            {
                TiketId = Guid.NewGuid().ToString(),

                Type = "CHAPTER",

                ChapterId = chapter.ChapterId,

                NovelId = chapter.NovelId,

                TargetUid = novel?.Uid,

                RepoterUid = model.ReporterUid,

                ReasonDetail = model.ReasonDetail,

                Status = "PENDING",

                TimeSend = DateTime.Now
            };

            _context.ReportTickets.Add(ticket);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Báo cáo chương thành công",
                ticketId = ticket.TiketId
            });
        }
    }
}