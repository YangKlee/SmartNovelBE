using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SmartNovelBE.Models;
using SmartNovelBE.Services;

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

    }
}
