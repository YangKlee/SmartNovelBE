using Microsoft.EntityFrameworkCore;
using SmartNovelBE.Models; // Đảm bảo đúng namespace Models của bạn
using SmartNovelBE.DTOs.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.Design;

namespace SmartNovelBE.Services
{
    public class HomeService : IHomeService
    {
        private readonly SmartTruyenDbContext _context;

        public HomeService(SmartTruyenDbContext context)
        {
            _context = context;
        }

  
        private async Task<(List<string> blockedCategories, List<string> blockedAuthors)> GetUserBlockListsAsync(string? userId)
        {
            if (string.IsNullOrEmpty(userId))
                return (new List<string>(), new List<string>());

            var currentUser = await _context.Users
                .Include(u => u.Categories)
                .Include(u => u.Authors)
                .FirstOrDefaultAsync(u => u.Uid == userId);

            var blockedCategoryIds = currentUser?.Categories.Select(c => c.CategoryId).ToList() ?? new List<string>();
            var blockedAuthorIds = currentUser?.Authors.Select(c => c.Uid).ToList() ?? new List<string>();

            return (blockedCategoryIds, blockedAuthorIds);
        }


        public async Task<NovelSummaryDto> GetFeaturedNovelAsync()
        {
            var featuredNovel = await _context.Novels
                .Where(n => n.Status != "Deleted")
                .OrderBy(x => Guid.NewGuid())
                .Select(n => new NovelSummaryDto
                {
                    NovelId = n.NovelId,
                    Title = n.Title,
                    Slug = n.Slug,
                    imageNovelUrl = n.ImageNovelUrl,
                    Status = n.Status,
                    ViewCount = n.ViewCount ?? 0,
                    LikeCount = n.LikeCount ?? 0,
                    AuthorName = n.UidNavigation.DisplayName,
                    AgeRating =n.AgeRating,
                    imageBanerNovelUrl=n.ImageBanerNovelUrl,
                    Description = n.Description,
                    CountChapter=n.Chapters.Count,
                    Categories = n.Categories.Select(c => c.Name).ToList()

                })
                .FirstOrDefaultAsync();

            return featuredNovel; 
        }

        public async Task<List<NovelSummaryDto>> GetHotNovelsAsync(string? currentUserId)
        {
            var (blockCategoryIds, blockAuthorIds) = await GetUserBlockListsAsync(currentUserId);

            return await _context.Novels
                .Where(n => n.Status == "Public" || n.Status == "Completed")
                .Where(n => !blockAuthorIds.Contains(n.Uid))
                .Where(n => !n.Categories.Any(c => blockCategoryIds.Contains(c.CategoryId)))
                .OrderByDescending(n => (n.ViewCount ?? 0) + (n.LikeCount ?? 0) * 10)
                .Take(10)
                .Select(n => new NovelSummaryDto
                {
                    NovelId = n.NovelId,
                    Title = n.Title,
                    Slug = n.Slug,
                    imageNovelUrl = n.ImageNovelUrl,
                    AuthorName = n.UidNavigation.DisplayName,
                    Status = n.Status,
                    ViewCount = n.ViewCount ?? 0,
                    LikeCount = n.LikeCount ?? 0,
                    AgeRating = n.AgeRating,
                    Description = n.Description,
                    CountChapter = n.Chapters.Count,
                    Categories = n.Categories.Select(c => c.Name).ToList()


                })
                .ToListAsync();
        }

        public async Task<List<NovelSummaryDto>> GetRecommendedNovelsAsync(string? currentUserId)
        {
            if (string.IsNullOrEmpty(currentUserId)) return new List<NovelSummaryDto>();

            var (blockCategoryIds, blockAuthorIds) = await GetUserBlockListsAsync(currentUserId);

            var followedAuthorIds = await _context.Users
                .Where(u => u.Uid == currentUserId)
                .SelectMany(u => u.UidsNavigation)
                .Select(u => u.Uid)
                .ToListAsync();

            var historyCategoryIds = await _context.HistoryReaders
                .Where(u => u.Uid == currentUserId)
                .SelectMany(u => u.Novel.Categories)
                .Select(c => c.CategoryId)
                .Distinct()
                .ToListAsync();

            var followedAuthorCategoryIds = await _context.Novels
                .Where(n => followedAuthorIds.Contains(n.Uid))
                .SelectMany(n => n.Categories)
                .Select(c => c.CategoryId)
                .Distinct()
                .ToListAsync();

            var targetCategoryIds = historyCategoryIds.Union(followedAuthorCategoryIds).Distinct().ToList();

            return await _context.Novels
                .Where(u => u.Status == "Public" || u.Status == "Completed")
                .Where(n => !blockAuthorIds.Contains(n.Uid))
                .Where(n => !n.Categories.Any(c => blockCategoryIds.Contains(c.CategoryId)))
                .Where(n => followedAuthorIds.Contains(n.Uid) || n.Categories.Any(c => targetCategoryIds.Contains(c.CategoryId)))
                .OrderByDescending(n => followedAuthorIds.Contains(n.Uid))
                .ThenByDescending(n => n.ViewCount)
                .Take(9)
                .Select(n => new NovelSummaryDto
                {
                    NovelId = n.NovelId,
                    Title = n.Title,
                    Slug = n.Slug,
                    imageNovelUrl = n.ImageNovelUrl,
                    AuthorName = n.UidNavigation.DisplayName,
                    Status = n.Status,
                    ViewCount = n.ViewCount ?? 0,
                    LikeCount = n.LikeCount ?? 0,
                    AgeRating = n.AgeRating,
                    Description = n.Description,
                    CountChapter = n.Chapters.Count,
                    Categories = n.Categories.Select(c=>c.Name).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<NovelSummaryDto>> GetAdminRecommendNovelsAsync()
        {
            return await _context.RecommendNovels
                .Select(r => r.Novel)
                .Take(5)
                .Select(n => new NovelSummaryDto
                {
                    NovelId = n.NovelId,
                    Title = n.Title,
                    Slug = n.Slug,
                    imageNovelUrl = n.ImageNovelUrl,
                    AuthorName = n.UidNavigation.DisplayName,
                    Status = n.Status,
                    ViewCount = n.ViewCount ?? 0,
                    LikeCount = n.LikeCount ?? 0,
                    AgeRating = n.AgeRating,
                    Description =n.Description,
                    CountChapter = n.Chapters.Count,
                    Categories = n.Categories.Select(c => c.Name).ToList()
                })
                .ToListAsync();
        }



        public async Task<List<NovelSummaryDto>> GetSidebarNewUpdateAsync(string? currentUserId)
        {
            var (blockCategoryIds, blockAuthorIds) = await GetUserBlockListsAsync(currentUserId);

            return await _context.Novels
                .Where(u => u.Status == "Public" || u.Status == "Completed")
                .Where(u => !u.Categories.Any(c => blockCategoryIds.Contains(c.CategoryId)))
                .Where(u => !blockAuthorIds.Contains(u.Uid))
                .OrderByDescending(u => u.Chapters.Max(c => c.CreateTime))
                .Take(9)
                .Select(n => new NovelSummaryDto
                {
                    NovelId = n.NovelId,
                    Title = n.Title,
                    Slug = n.Slug,
                    imageNovelUrl = n.ImageNovelUrl,
                    AuthorName = n.UidNavigation.DisplayName,
                    Status = n.Status,
                    ViewCount = n.ViewCount ?? 0,
                    LikeCount = n.LikeCount ?? 0,
                    AgeRating = n.AgeRating,
                    Description = n.Description,
                    CountChapter = n.Chapters.Count,
                    Categories = n.Categories.Select(c => c.Name).ToList()

                })
                .ToListAsync();
        }

        public async Task<List<AuthorSummaryDto>> GetTopAuthorsAsync()
        {
            string RoleAuthorId = "3";
            string StatusUser = "Active";
            return await _context.Users
                .Where(u => u.RoleId == RoleAuthorId && u.Status == StatusUser)
                .OrderByDescending(u => u.CreatorPoint)
                .Take(5)
                .Select(u => new AuthorSummaryDto
                {
                    Uid = u.Uid,
                    DisplayName = u.DisplayName,
                    AvatarUrl = u.AvartarUrl, 
                    CreatorPoint = u.CreatorPoint ?? 0
                })
                .ToListAsync();
        }
    }
}