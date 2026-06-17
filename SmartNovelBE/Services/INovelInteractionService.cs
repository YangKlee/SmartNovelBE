using SmartNovelBE.DTOs.Novel;

namespace SmartNovelBE.Services
{
    public interface INovelInteractionService
    {
        Task<bool> FollowNovelAsync(string currentUid, string novelId);

        Task<bool> UnFollowNovelAsync( string currentUid, string novelId);

        Task<List<FollowingNovelDto>> GetFollowingNovelsAsync(string currentUid);

        Task<RateNovelResponse> RateNovelAsync(string currentUid,RateNovelRequest request);
        Task<int?> GetMyRatingAsync(string currentUid, string novelId);
    }
}
