using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SmartNovelBE.Services;

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

        
        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // 1 GET api/HomeApi/featured
        [HttpGet("featured")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFeaturedNovel()
        {
            var data = await _homeService.GetFeaturedNovelAsync();
            return Ok(data);
        }

        // 2 GET api/HomeApi/hot
        [HttpGet("hot")]
        [AllowAnonymous] 
        public async Task<IActionResult> GetHotNovels()
        {
            string? userId = GetCurrentUserId();
            var data = await _homeService.GetHotNovelsAsync(userId);
            return Ok(data);
        }

        //3 GET api/HomeApi/recommended
        [HttpGet("recommended")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRecommendedNovels()
        {
            string? userId = GetCurrentUserId();
            var data = await _homeService.GetRecommendedNovelsAsync(userId);
            return Ok(data);
        }

        // GET api/HomeApi/admin-recommend
        [HttpGet("admin-recommend")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAdminRecommend()
        {
            var data = await _homeService.GetAdminRecommendNovelsAsync();

            return Ok(data);
        }

        // 5 GET api/HomeApi/sidebar-new-update
        [HttpGet("sidebar-new-update")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSidebarNewUpdate()
        {
            string? userId = GetCurrentUserId();
             var data = await _homeService.GetSidebarNewUpdateAsync(userId);
            return Ok(data);
        }

        // 6.GET api/HomeApi/top-authors
        [HttpGet("top-authors")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopAuthors()
        {
            var data = await _homeService.GetTopAuthorsAsync();

            return Ok(data);
        }
    }
}