using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNovelBE.Models;
using SmartNovelBE.Services;

namespace SmartNovelBE.Controllers
{
    [Route("api/user-block")]
    [ApiController]
    [Authorize]
    public class UserBlockController : ControllerBase
    {
        private readonly IUserBlockService _userBlockService;

        public UserBlockController(IUserBlockService userBlockService)
        {
            _userBlockService = userBlockService;
        }

        [HttpPost("block/{targetUid}")]
        public async Task<IActionResult> BlockUser(string targetUid)
        {
            var uid = User.FindFirst("Uid")?.Value;

            var result =await _userBlockService.BlockUserAsync(uid!,targetUid);

            if (!result)
                return BadRequest();

            return Ok();
        }

        [HttpDelete("unblock/{targetUid}")]
        public async Task<IActionResult> UnBlockUser(string targetUid)
        {
            var uid = User.FindFirst("Uid")?.Value;

            var result = await _userBlockService.UnBlockUserAsync(uid!, targetUid);

            if (!result)
                return BadRequest();

            return Ok();
        }

        [HttpGet("blocked-users")]
        public async Task<IActionResult>GetBlockedUsers()
        {
            var uid = User.FindFirst("Uid")?.Value;

            var result = await _userBlockService.GetBlockedUsersAsync(uid!);

            return Ok(result);
        }
    }
}
