using Microsoft.EntityFrameworkCore;
using SmartNovelBE.DTOs.Novel;
using SmartNovelBE.DTOs.User;
using SmartNovelBE.Models;

namespace SmartNovelBE.Services
{
    public class UserRelationService : IUserRelationService
    {
        private readonly SmartTruyenDbContext _context;

        public UserRelationService(SmartTruyenDbContext context)
        {
            _context = context;
        }

        public async Task<bool> FollowAsync(string currentUid,string targetUid)
        {
            if (currentUid == targetUid)
                return false;

            var targetUser = await _context.Users
                .Include(x => x.FollowerUs)
                .FirstOrDefaultAsync(x => x.Uid == targetUid);

            var currentUser = await _context.Users
                .FindAsync(currentUid);

            if (targetUser == null || currentUser == null)
                return false;

            if (targetUser.FollowerUs.Any(x => x.Uid == currentUid))
                return false;

            targetUser.FollowerUs.Add(currentUser);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UnFollowAsync(string currentUid,string targetUid)
        {
            var targetUser = await _context.Users
                .Include(x => x.FollowerUs)
                .FirstOrDefaultAsync(x => x.Uid == targetUid);

            if (targetUser == null)
                return false;

            var follower = targetUser.FollowerUs
                .FirstOrDefault(x => x.Uid == currentUid);

            if (follower == null)
                return false;

            targetUser.FollowerUs.Remove(follower);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsFollowingAsync(string currentUid, string targetUid)
        {
            return await _context.Users
                .Where(x => x.Uid == targetUid)
                .AnyAsync(x =>
                    x.FollowerUs.Any(f =>f.Uid == currentUid));
        }

        public async Task<List<UserSimpleDto>> GetFollowersAsync(string uid)
        {
            var user = await _context.Users
                .Include(x => x.FollowerUs)
                .FirstOrDefaultAsync(x => x.Uid == uid);

            if (user == null)
                return new();

            return user.FollowerUs
                .Select(x => new UserSimpleDto
                {
                    Uid = x.Uid,
                    Username = x.Username,
                    DisplayName = x.DisplayName,
                    AvatarUrl = x.AvartarUrl
                })
                .ToList();
        }

        public async Task<List<UserSimpleDto>> GetFollowingAsync(
            string uid)
        {
            var user = await _context.Users
                .Include(x => x.UidsNavigation)
                .FirstOrDefaultAsync(x => x.Uid == uid);

            if (user == null)
                return new();

            return user.UidsNavigation
                .Select(x => new UserSimpleDto
                {
                    Uid = x.Uid,
                    Username = x.Username,
                    DisplayName = x.DisplayName,
                    AvatarUrl = x.AvartarUrl
                })
                .ToList();
        }

        public async Task<bool> BlockAsync(string currentUid,string targetUid)
        {
            if (currentUid == targetUid)
                return false;

            var user = await _context.Users
                .Include(x => x.Authors)
                .FirstOrDefaultAsync(x => x.Uid == currentUid);

            var target = await _context.Users
                .FindAsync(targetUid);

            if (user == null || target == null)
                return false;

            if (user.Authors.Any(x => x.Uid == targetUid))
                return false;

            user.Authors.Add(target);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UnBlockAsync(string currentUid,string targetUid)
        {
            var user = await _context.Users
                .Include(x => x.Authors)
                .FirstOrDefaultAsync(x => x.Uid == currentUid);

            if (user == null)
                return false;

            var target = user.Authors
                .FirstOrDefault(x => x.Uid == targetUid);

            if (target == null)
                return false;

            user.Authors.Remove(target);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsBlockedAsync(string currentUid,string targetUid)
        {
            return await _context.Users
                .Where(x => x.Uid == currentUid)
                .AnyAsync(x =>
                    x.Authors.Any(a => a.Uid == targetUid));
        }

        public async Task<List<UserSimpleDto>> GetBlockedUsersAsync(string uid)
        {
            var user = await _context.Users
                .Include(x => x.Authors)
                .FirstOrDefaultAsync(x => x.Uid == uid);

            if (user == null)
                return new();

            return user.Authors
                .Select(x => new UserSimpleDto
                {
                    Uid = x.Uid,
                    Username = x.Username,
                    DisplayName = x.DisplayName,
                    AvatarUrl = x.AvartarUrl
                })
                .ToList();
        }

        public async Task<UserProfileDto> GetProfileAsync(string currentUid, string targetUid)
        {
            var user = await _context.Users
                .Where(x => x.Uid == targetUid)
                .Select(x => new UserProfileDto
                {
                    Uid = x.Uid,
                    Username = x.Username,
                    DisplayName = x.DisplayName,
                    AvatarUrl = x.AvartarUrl,
                    RoleId = x.RoleId,

                    FollowerCount = x.FollowerUs.Count,
                    FollowingCount = x.UidsNavigation.Count,

                    IsSelf = currentUid == targetUid,

                    IsFollowing = x.FollowerUs.Any(f => f.Uid == currentUid),
                    IsBlocked = x.Authors.Any(a => a.Uid == currentUid)
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return null;

            // =========================
            // ROLE 3 - TÁC GIẢ
            // =========================
            if (user.RoleId == "3")
            {
                user.TotalViews = await _context.Novels
                    .Where(n => n.Uid == targetUid)
                    .SumAsync(n => (long?)n.ViewCount) ?? 0;

                user.NovelCount = await _context.Novels
                    .CountAsync(n => n.Uid == targetUid);

                user.Novels = await _context.Novels
                    .Where(n => n.Uid == targetUid)
                    .Select(n => new NovelListDto
                    {
                        NovelId = n.NovelId,
                        Title = n.Title,
                        ImageNovelUrl = n.ImageNovelUrl,
                        ViewCount = n.ViewCount ?? 0,
                        LikeCount = n.LikeCount ?? 0,
                        Status = n.Status,
                        CreateTime = n.CreateTime
                    })
                    .ToListAsync();
            }

            // =========================
            // FOLLOWED NOVELS (ROLE 3 + 4)
            // =========================
            user.FollowedNovels = await _context.Novels
                .Where(n => n.Uids.Any(u => u.Uid == targetUid))
                .Select(n => new NovelListDto
                {
                    NovelId = n.NovelId,
                    Title = n.Title,
                    ImageNovelUrl = n.ImageNovelUrl,
                    ViewCount = n.ViewCount ?? 0,
                    LikeCount = n.LikeCount ?? 0
                })
                .ToListAsync();

            // =========================
            // FOLLOWERS
            // =========================
            user.Followers = await _context.Users
                .Where(u => u.FollowerUs.Any(f => f.Uid == targetUid))
                .Select(u => new UserSimpleDto
                {
                    Uid = u.Uid,
                    Username = u.Username,
                    DisplayName = u.DisplayName,
                    AvatarUrl = u.AvartarUrl
                })
                .ToListAsync();

            // =========================
            // FOLLOWING
            // =========================
            user.Following = await _context.Users
                .Where(u => u.UidsNavigation.Any(f => f.Uid == targetUid))
                .Select(u => new UserSimpleDto
                {
                    Uid = u.Uid,
                    Username = u.Username,
                    DisplayName = u.DisplayName,
                    AvatarUrl = u.AvartarUrl
                })
                .ToListAsync();

            return user;
        }
    }
}

