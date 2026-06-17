using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartNovelBE.Models;
using SmartNovelBE.Models.DTOs;

namespace SmartNovelBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NovelReportController : ControllerBase
    {
        private readonly SmartTruyenDbContext _context;

        public NovelReportController(
            SmartTruyenDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport(
            NovelReportDto model)
        {
            var novel = await _context.Novels
                .FirstOrDefaultAsync(x =>
                    x.NovelId == model.NovelId);

            if (novel == null)
            {
                return NotFound("Novel not found");
            }

            var ticket = new ReportTicket
            {
                TiketId = Guid.NewGuid().ToString(),

                Type = "NOVEL",

                NovelId = novel.NovelId,

                TargetUid = novel.Uid,

                RepoterUid = model.ReporterUid,

                ReasonDetail = model.ReasonDetail,

                Status = "PENDING",

                TimeSend = DateTime.Now
            };

            _context.ReportTickets.Add(ticket);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Báo cáo truyện thành công",
                ticketId = ticket.TiketId
            });
        }
    }
}