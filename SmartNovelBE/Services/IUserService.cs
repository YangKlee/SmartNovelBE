using SmartNovelBE.DTOs.AdminUser;
using SmartNovelBE.Models;

namespace SmartNovelBE.Services
{
    public interface IUserService
    {
        Task<object> GetUsersAsync(string? keyword, string? role, string? status, int page, int pageSize = 10);
        Task<User> CreateUserAsync(CreateUserDto model);
        Task<User> UpdateUserAsync(UpdateUserDto model);
        Task<bool> DeleteUserAsync(string id);
    }


}