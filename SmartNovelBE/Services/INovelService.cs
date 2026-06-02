using SmartNovelBE.DTOs.Novel;
using SmartNovelBE.DTOs.Chapter;
namespace SSmartNovelBE.Services.Interfaces
{
    public interface INovelService
    {
        Task<NovelDetailDto?> GetBySlugAsync(string slug);
        Task<List<ChapterListDto>>GetChaptersByNovelIdAsync(string novelId);
    }
}
