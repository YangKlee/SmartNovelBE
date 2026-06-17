using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNovelBE.Models;
using SmartNovelBE.DTOs.Novel;
using SmartNovelBE.Services;

namespace SmartNovelBE.Controllers
{
    [Route("api/novel-interaction")]
    [ApiController]
    public class NovelInteractionController : ControllerBase
    {
        private readonly INovelInteractionService _novelInteractionService;

        public NovelInteractionController( INovelInteractionService novelInteractionService)
        {
            _novelInteractionService = novelInteractionService;
        }

        //Theo dõi truyện
        [Authorize]
        [HttpPost("follow/{novelId}")]
        public async Task<IActionResult> FollowNovel(string novelId)
        {
            var uid = User.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(uid))
            {
                return Unauthorized("Không tìm thấy uid trong token");
            }

            try
            {
                var result = await _novelInteractionService
                    .FollowNovelAsync(uid, novelId);

                if (!result)
                {
                    return BadRequest(new
                    {
                        Message = "Không thể theo dõi truyện",
                        Uid = uid,
                        NovelId = novelId
                    });
                }

                return Ok(new
                {
                    Message = "Theo dõi truyện thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
        }

        // Bỏ theo dõi truyện
        [Authorize]
        [HttpDelete("unfollow/{novelId}")] 
        public async Task<IActionResult> UnFollowNovel(string novelId)
        {
            var uid = User.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(uid))
            {
                return Unauthorized(new { Message = "Không tìm thấy uid trong token" });
            }

            try
            {
                var result = await _novelInteractionService.UnFollowNovelAsync(uid, novelId);

                if (!result)
                {
                    return BadRequest(new { Message = "Không thể bỏ theo dõi hoặc bản ghi không tồn tại" });
                }

                return Ok(new { Message = "Bỏ theo dõi thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // Danh sách truyện đang theo dõi
        [Authorize]
        [HttpGet("following")]
        public async Task<IActionResult> GetFollowingNovels()
        {
            var uid = User.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(uid))
            {
                return Unauthorized();
            }

            var result = await _novelInteractionService
                .GetFollowingNovelsAsync(uid);

            return Ok(result);
        }
        // Đánh giá truyện

        [Authorize]
        [HttpPost("rate")]
        public async Task<IActionResult> RateNovel([FromBody] RateNovelRequest request)
        {
            var uid = User.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(uid))
            {
                return Unauthorized();
            }

            var result = await _novelInteractionService.RateNovelAsync(uid, request);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("my-rating/{novelId}")]
        public async Task<IActionResult> GetMyRating(string novelId)
        {
            var uid = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(uid))
                return Unauthorized();
            return Ok(await _novelInteractionService.GetMyRatingAsync(uid, novelId));
        }
    }
}