using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartNovelBE.Models;
using SmartNovelBE.Models.DTOs;

namespace SmartNovelBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentReportController : ControllerBase
    {
        private readonly SmartTruyenDbContext _context;

        public CommentReportController(
            SmartTruyenDbContext context)
        {
            _context = context;
        }

        // BÁO CÁO BÌNH LUẬN
        [HttpPost("create")]
        public async Task<IActionResult> CreateReport(
            [FromBody] CommentReportDto model)
        {
            if (model == null)
            {
                return BadRequest("Data is required");
            }

            var comment = await _context.Comments
                .FirstOrDefaultAsync(c =>
                    c.CommentId == model.CommentId);

            if (comment == null)
            {
                return NotFound("Comment not found");
            }

            var ticket = new ReportTicket
            {
                TiketId = Guid.NewGuid().ToString(),
                Type = "COMMENT",
                CommentId = comment.CommentId,
                TargetUid = comment.Uid,
                RepoterUid = model.ReporterUid,
                ReasonDetail = model.ReasonDetail,
                Status = "PENDING",
                TimeSend = DateTime.Now
            };

            _context.ReportTickets.Add(ticket);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Báo cáo thành công",
                ticketId = ticket.TiketId
            });
        }
    }
}