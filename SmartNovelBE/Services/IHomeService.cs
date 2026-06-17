using SmartNovelBE.DTOs.Home;

namespace SmartNovelBE.Services
{
    public interface IHomeService
    {
      
        Task<NovelSummaryDto> GetFeaturedNovelAsync(); 
        Task<List<NovelSummaryDto>> GetHotNovelsAsync(string? currentUserId); 
        Task<List<NovelSummaryDto>> GetRecommendedNovelsAsync(string? currentUserId); 
        Task<List<NovelSummaryDto>> GetAdminRecommendNovelsAsync(); 
        Task<List<NovelSummaryDto>> GetSidebarNewUpdateAsync(string? currentUserId); 
        Task<List<AuthorSummaryDto>> GetTopAuthorsAsync();

        Task<List<NovelSummaryDto>> GetNovelFlowingAsync(string ?currenUserId);
    }
}