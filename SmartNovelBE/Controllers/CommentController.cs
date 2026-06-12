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
    public class CommentController : ControllerBase
    {
        private readonly JwtServices _jwtServices;
        private readonly SmartTruyenDbContext _context;
        private readonly MailServices _mailServices;
        private readonly IMemoryCache _cache;
        public CommentController(JwtServices jwtServices, SmartTruyenDbContext context, MailServices mailServices, IMemoryCache cache)
        {
            _jwtServices = jwtServices;
            _context = context;
            _mailServices = mailServices;
            _cache = cache;
        }
        [HttpGet("getComment")]

        public async Task<IActionResult> getComment([FromQuery] string novelID, [FromQuery] string chapterId, 
            [FromQuery] int currentComment = 0, [FromQuery] int  limitComment = 5, [FromQuery] string? parentComment = null)
        {
            var roleId = User.FindFirstValue(ClaimTypes.Role);
            var currentUserId = User.FindFirstValue("uid");

            var commentsQuery = _context.Comments
                .Include(c => c.Chapter)
                    .ThenInclude(c => c.Novel)
                .Include(c => c.UidNavigation)
                .Include(c => c.InverseParentComment)
                .Where(c => c.Chapter.Novel.NovelId == novelID && c.ChapterId == chapterId);

            if (string.IsNullOrEmpty(parentComment))
            {
                commentsQuery = commentsQuery.Where(c => c.ParentCommentId == null);
            }
            else
            {
                commentsQuery = commentsQuery.Where(c => c.ParentCommentId == parentComment);
            }

            var comments = await commentsQuery
                .OrderByDescending(c => c.TimeCommeny)
                .Skip(currentComment)
                .Take(limitComment)
                .Select(c => new UserRespone.CommentResponse
                {
                    CommentId = c.CommentId,
                    NovelId = c.Chapter.Novel.NovelId,
                    ChapterId = c.ChapterId,
                    ParentCommentId = c.ParentCommentId,
                    UserId = c.Uid,
                    Content = c.Content,
                    DisplayName = c.UidNavigation.DisplayName,
                    UserAvatarUrl = c.UidNavigation.AvartarUrl,
                    CommentDateTime = c.TimeCommeny,
                    CurrentUserId = currentUserId,
                    RoleId = c.UidNavigation.RoleId,
                    IsAdminMode = (roleId == "1" || roleId == "2") ? true : false, 
                    CountChildComment = c.InverseParentComment.Count()
                })
                .ToListAsync();
            var res = new UserRespone.CommentReponeReal
            {
                comments = comments,
                totalComment = commentsQuery.Count()
            };
            return Ok(res);
        }
        [Authorize]
        [HttpPost("addComment")]
        public async Task<IActionResult> addComment([FromBody] UserRequests.addComment newCmt)
        {
            var currentUserId = User.FindFirstValue("uid");
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new { Msg = "Vui lòng đăng nhập" });
            }

            // Xử lý parentComment: nếu chuỗi rỗng thì gán là null
            string? parentCommentId = string.IsNullOrEmpty(newCmt.parentComment) ? null : newCmt.parentComment;

            var newComment = new Comment
            {
                CommentId = Guid.NewGuid().ToString(),
                ChapterId = newCmt.chapterId,
                Content = newCmt.content,
                ParentCommentId = parentCommentId,
                Status = "Active",
                Uid = currentUserId,
                //CommentId = Guid.NewGuid().ToString(),
                TimeCommeny = DateTime.Now
            };

            try
            {
                _context.Comments.Add(newComment);
                await _context.SaveChangesAsync();

                return Ok(new { Msg = "Thêm bình luận thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Msg = "Lỗi khi thêm bình luận: " + ex.Message });
            }
        }
        [Authorize]
        [HttpDelete("deleteComment")]
        public async Task<IActionResult> deleteComment([FromBody] string commentID)
        {
            var currentUserId = User.FindFirstValue("uid");
            var roleId = User.FindFirstValue(ClaimTypes.Role);

            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentID);
            if (comment == null)
                return BadRequest(new { Msg = "Comment không hợp lệ" });
            
            if (comment.Uid != currentUserId && (roleId == "3" || roleId == "4"))
                return BadRequest(new { Msg = "Không có quyền xóa comment này" });

            try
            {
                // Dùng vòng lặp để lấy toàn bộ các comment con, cháu... (tránh lỗi FK đệ quy nhiều tầng)
                var allCommentsToDelete = new List<Comment> { comment };
                var queue = new Queue<string>();
                queue.Enqueue(commentID);

                while (queue.Count > 0)
                {
                    var currentId = queue.Dequeue();
                    var children = await _context.Comments.Where(c => c.ParentCommentId == currentId).ToListAsync();
                    foreach (var child in children)
                    {
                        allCommentsToDelete.Add(child);
                        queue.Enqueue(child.CommentId);
                    }
                }

                // Xóa từ dưới lên trên (đảo ngược danh sách để xóa con trước)
                allCommentsToDelete.Reverse();
                _context.Comments.RemoveRange(allCommentsToDelete);
                await _context.SaveChangesAsync();
                
                return Ok(new { Msg = "Xóa bình luận thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Msg = "Lỗi khi xóa bình luận: " + ex.Message });
            }
        }
    }

}
