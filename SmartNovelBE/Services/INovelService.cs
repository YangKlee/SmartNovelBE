using SmartNovelBE.DTOs.Chapter;
using SmartNovelBE.DTOs.Home;
using SmartNovelBE.DTOs.Novel;
namespace SSmartNovelBE.Services.Interfaces
{
    public interface INovelService
    {
        Task<NovelDetailDto?> GetByNovelIdAsync(string novelId);
        Task<List<ChapterListDto>>GetChaptersByNovelIdAsync(string novelId);

        
    }

}
