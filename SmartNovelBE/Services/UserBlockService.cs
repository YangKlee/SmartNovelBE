using Microsoft.EntityFrameworkCore;
using SmartNovelBE.Models;

namespace SmartNovelBE.Services
{
    public class UserBlockService : IUserBlockService
    {
        private readonly SmartTruyenDbContext _context;

        public UserBlockService(
            SmartTruyenDbContext context)
        {
            _context = context;
        }

        public async Task<bool> BlockUserAsync(string currentUid,string targetUid)
        {
            if (currentUid == targetUid)
                return false;

            var currentUser = await _context.Users
                .Include(x => x.Authors)
                .FirstOrDefaultAsync(x => x.Uid == currentUid);

            var targetUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Uid == targetUid);

            if (currentUser == null || targetUser == null)
                return false;

            bool alreadyBlocked = currentUser.Authors
                .Any(x => x.Uid == targetUid);

            if (alreadyBlocked)
                return false;

            currentUser.Authors.Add(targetUser);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UnBlockUserAsync(string currentUid,string targetUid)
        {
            var currentUser = await _context.Users
        .Include(x => x.Authors)
        .FirstOrDefaultAsync(x => x.Uid == currentUid);

            if (currentUser == null)
                return false;

            var blockedUser = currentUser.Authors
                .FirstOrDefault(x => x.Uid == targetUid);

            if (blockedUser == null)
                return false;

            currentUser.Authors.Remove(blockedUser);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<BlockedUserDto>>
            GetBlockedUsersAsync(string currentUid)
        {
            var user = await _context.Users
        .Include(x => x.Authors)
        .FirstOrDefaultAsync(x => x.Uid == currentUid);

            if (user == null)
                return new List<BlockedUserDto>();

            return user.Authors
                .Select(x => new BlockedUserDto
                {
                    Uid = x.Uid,
                    Username = x.Username,
                    DisplayName = x.DisplayName,
                    AvatarUrl = x.AvartarUrl,
                    RoleId = x.RoleId,
                    Status = x.Status
                })
                .ToList();
        }
    }
}
