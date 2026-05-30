using Microsoft.AspNetCore.Mvc;
using SSmartNovelBE.Services.Interfaces;

namespace SmartNovelBE.Controllers
{
    [ApiController]
    [Route("api/novel")]
    public class NovelController : ControllerBase
    {
        private readonly INovelService _novelService;

        public NovelController(
            INovelService novelService)
        {
            _novelService = novelService;
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult>
            GetDetail(string slug)
        {
            var result =
                await _novelService
                    .GetBySlugAsync(slug);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
        [HttpGet("{novelId}/chapters")]
        public async Task<IActionResult>
        GetChapters(string novelId)
        {
            var result =
                await _novelService
                    .GetChaptersByNovelIdAsync(
                        novelId);
            return Ok(result);
        }
    }
}
