using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNovelBE.Services;

namespace SmartNovelBE.Controllers
{
    [Route("api/profile")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(
            IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("{uid}")]
        public async Task<IActionResult> GetProfile(
            string uid)
        {
            string? currentUid = User.FindFirst("uid")?.Value;

            var result =
                await _profileService
                    .GetProfileAsync(uid, currentUid);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var uid = User.FindFirst("uid")?.Value;

            var result =
                await _profileService
                    .GetMyProfileAsync(uid!);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("follow-author/{authorUid}")]
        public async Task<IActionResult> FollowAuthor(
            string authorUid)
        {
            var uid = User.FindFirst("uid")?.Value;

            var result =
                await _profileService
                    .FollowAuthorAsync(
                        uid!,
                        authorUid);

            if (!result)
                return BadRequest();

            return Ok(new
            {
                success = true
            });
        }

        [Authorize]
        [HttpDelete("unfollow-author/{authorUid}")]
        public async Task<IActionResult> UnFollowAuthor(
            string authorUid)
        {
            var uid = User.FindFirst("uid")?.Value;

            var result =
                await _profileService
                    .UnFollowAuthorAsync(
                        uid!,
                        authorUid);

            if (!result)
                return BadRequest();

            return Ok();
        }

        [Authorize]
        [HttpGet("following-authors")]
        public async Task<IActionResult>
            GetFollowingAuthors()
        {
            var uid = User.FindFirst("uid")?.Value;

            var result =
                await _profileService
                    .GetFollowingAuthorsAsync(
                        uid!);

            return Ok(result);
        }
    }
}
