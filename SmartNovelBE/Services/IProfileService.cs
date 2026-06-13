using SmartNovelBE.Models;

namespace SmartNovelBE.Services
{
    public interface IProfileService
    {
        Task<ProfileResponse?> GetProfileAsync(string profileUid,string? currentUid);

        Task<ProfileResponse?> GetMyProfileAsync(string currentUid);

        Task<bool> FollowAuthorAsync(string currentUid,string authorUid);

        Task<bool> UnFollowAuthorAsync(string currentUid,string authorUid);

        Task<List<AuthorFollowDto>>
            GetFollowingAuthorsAsync(string currentUid);
    }
}
