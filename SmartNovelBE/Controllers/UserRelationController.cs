using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNovelBE.Models;
using SmartNovelBE.Services;
using System.Security.Claims;

namespace SmartNovelBE.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/user-relation")]
    public class UserRelationController : ControllerBase
        {
            private readonly IUserRelationService _service;

            public UserRelationController(IUserRelationService service)
            {
                _service = service;
            }

        // Theo dõi một người dùng
            [HttpPost("follow/{uid}")]
            public async Task<IActionResult> Follow(string uid)
            {
                var currentUid = User.FindFirst("Uid")?.Value;

                return Ok(await _service.FollowAsync(currentUid!,uid));
            }

        // Bỏ theo dõi một người dùng
        [HttpDelete("follow/{uid}")]
            public async Task<IActionResult> UnFollow(string uid)
            {
                var currentUid = User.FindFirst("Uid")?.Value;

                return Ok(await _service.UnFollowAsync(currentUid!,uid));
            }
        // Kiểm tra người dùng hiện tại có đang follow uid hay không
        [HttpGet("is-following/{uid}")]
            public async Task<IActionResult> IsFollowing(string uid)
            {
                var currentUid = User.FindFirst("Uid")?.Value;

                return Ok(await _service.IsFollowingAsync(currentUid!,uid));
            }
        // Lấy danh sách người đang follow user này
        [HttpGet("followers/{uid}")]
            public async Task<IActionResult> Followers(string uid)
            {
                return Ok(await _service.GetFollowersAsync(uid));
            }
        // Lấy danh sách user mà người này đang follow
        [HttpGet("following/{uid}")]
            public async Task<IActionResult> Following(string uid)
            {
                return Ok(await _service.GetFollowingAsync(uid));
            }
        // Chặn một người dùng
        [HttpPost("block/{uid}")]
            public async Task<IActionResult> Block(string uid)
            {
                var currentUid = User.FindFirst("uid")?.Value;

                return Ok(await _service.BlockAsync(currentUid!,uid));
            }
        // Bỏ chặn một người dùng
        [HttpDelete("block/{uid}")]
            public async Task<IActionResult> UnBlock(string uid)
            {
            var currentUid = User.FindFirst("uid")?.Value;

            return Ok(await _service.UnBlockAsync(currentUid!,uid));
            }
        // Kiểm tra đã block user này chưa
        [HttpGet("is-blocked/{uid}")]
        public async Task<IActionResult> IsBlocked(string uid)
        {
            var currentUid = User.FindFirst("uid")?.Value;

            return Ok(await _service.IsBlockedAsync(currentUid!, uid));
        }
        // Lấy danh sách người dùng đã bị block
        [HttpGet("blocked")]
            public async Task<IActionResult> GetBlockedUsers()
            {
            var currentUid = User.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(currentUid))
                    return Unauthorized();

                return Ok(await _service.GetBlockedUsersAsync(currentUid));
            }
    }
}
