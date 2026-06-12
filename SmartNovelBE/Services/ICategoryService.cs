using SmartNovelBE.Models;

namespace SmartNovelBE.Services
{
    public interface ICategoryService
    {
        Task<object> GetCategoriesAsync(string? keyword, string? status, int page);
        Task<Category> CreateCategoryAsync(Category model);
        Task<Category> UpdateCategoryAsync(Category model);
        Task<bool> DeleteCategoryAsync(string id);
    }
}
