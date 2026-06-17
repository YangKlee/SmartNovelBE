using SmartNovelBE.DTOs.User;

namespace SmartNovelBE.Services
{
    public interface IUserRelationService
    {
        //Follow người dùng
        Task<bool> FollowAsync(string currentUid, string targetUid);
        Task<bool> UnFollowAsync(string currentUid, string targetUid);
        Task<bool> IsFollowingAsync(string currentUid, string targetUid);
        //Lấy danh sách người theo dõi
        Task<List<UserSimpleDto>> GetFollowersAsync(string uid);
        //Lấy danh sách người đang theo dõi
        Task<List<UserSimpleDto>> GetFollowingAsync(string uid);
        //Block người dùng
        Task<bool> BlockAsync(string currentUid, string targetUid);
        Task<bool> UnBlockAsync(string currentUid, string targetUid);
        Task<bool> IsBlockedAsync(string currentUid, string targetUid);
        Task<List<UserSimpleDto>> GetBlockedUsersAsync(string uid);

        // Get user profile with relation info
        Task<UserProfileDto?> GetProfileAsync(string currentUid, string targetUid);
    }
}
