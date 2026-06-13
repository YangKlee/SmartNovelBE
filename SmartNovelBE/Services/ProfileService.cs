using SmartNovelBE.Models;

namespace SmartNovelBE.Services
{
    public class ProfileService : IProfileService
    {
        private readonly SmartTruyenDbContext _context;

        public ProfileService(
            SmartTruyenDbContext context)
        {
            _context = context;
        }

        public async Task<ProfileResponse?> GetProfileAsync(string profileUid, string? currentUid)
        {
            throw new NotImplementedException();
        }

        public async Task<ProfileResponse?> GetMyProfileAsync(string currentUid)
        {
            return await GetProfileAsync(currentUid, currentUid);
        }

        public async Task<bool> FollowAuthorAsync(string currentUid,string authorUid)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UnFollowAuthorAsync(string currentUid,string authorUid)
        {
            throw new NotImplementedException();
        }

        public async Task<List<AuthorFollowDto>>
            GetFollowingAuthorsAsync( string currentUid)
        {
            throw new NotImplementedException();
        }
    }
}
