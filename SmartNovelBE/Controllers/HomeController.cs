using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Configuration;
using SmartNovelBE.Services;
using System.Security.Claims;

namespace SmartNovelBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeApiController : ControllerBase
    {
        private readonly IHomeService _homeService;

        public HomeApiController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        [HttpGet("featured")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFeaturedNovel()
        {
            var data = await _homeService.GetFeaturedNovelAsync();
            return Ok(data);
        }

        [HttpGet("hot")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHotNovels()
        {
            string? userId = User.FindFirst("uid")?.Value;
            var data = await _homeService.GetHotNovelsAsync(userId);
            return Ok(data);
        }

        [HttpGet("recommended")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRecommendedNovels()
        {
            string? userId = User.FindFirst("uid")?.Value;
            var data = await _homeService.GetRecommendedNovelsAsync(userId);
            return Ok(data);
        }

        [HttpGet("admin-recommend")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAdminRecommend()
        {
            var data = await _homeService.GetAdminRecommendNovelsAsync();

            return Ok(data);
        }

        [HttpGet("sidebar-new-update")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSidebarNewUpdate()
        {
            string? userId = User.FindFirst("uid")?.Value;
            var data = await _homeService.GetSidebarNewUpdateAsync(userId);
            return Ok(data);
        }

        [HttpGet("top-authors")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopAuthors()
        {
            var data = await _homeService.GetTopAuthorsAsync();

            return Ok(data);
        }

        [HttpGet("followedNovel")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFollowedNovel()
        {
            string? userId = User.FindFirst("uid")?.Value;
            var data = await _homeService.GetNovelFlowingAsync(userId);

            return Ok(data);


        }

    }
}