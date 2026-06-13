using SmartNovelBE.Models;
namespace SmartNovelBE.Services
{
    public interface IUserBlockService
    {
        Task<bool> BlockUserAsync( string currentUid, string targetUid);

        Task<bool> UnBlockUserAsync( string currentUid, string targetUid);

        Task<List<BlockedUserDto>>
            GetBlockedUsersAsync(string currentUid);
    }
}
