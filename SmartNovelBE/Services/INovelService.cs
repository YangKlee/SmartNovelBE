using SmartNovelBE.DTOs.Chapter;
using SmartNovelBE.DTOs.Home;
using SmartNovelBE.DTOs.Novel;
namespace SSmartNovelBE.Services.Interfaces
{
    public interface INovelService
    {
        Task<NovelDetailDto?> GetBySlugAsync(string slug);
        Task<List<ChapterListDto>>GetChaptersByNovelIdAsync(string novelId);

        
    }

}
