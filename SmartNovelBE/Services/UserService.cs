using Microsoft.EntityFrameworkCore;
using SmartNovelBE.DTOs.AdminUser;
using SmartNovelBE.Models;
using SmartNovelBE.Services;

namespace SmartNovel.Services
{
    public class UserService : IUserService
    {
        private readonly SmartTruyenDbContext _context;

        public UserService(SmartTruyenDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetUsersAsync(string keyword, string role, string status, int page, int pageSize = 10)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(u => u.DisplayName.Contains(keyword) || u.Username.Contains(keyword) || u.Email.Contains(keyword));

            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.RoleId == role);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(u => u.Status == status);

            int totalUsers = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Trả về một object chứa cả dữ liệu lẫn thông tin phân trang
            return new { Data = users, CurrentPage = page, TotalPages = totalPages, TotalUsers = totalUsers };
        }

        public async Task<User> CreateUserAsync(CreateUserDto model)
        {
            bool isExists = await _context.Users.AnyAsync(u => u.Username == model.Username || u.Email == model.Email);
            if (isExists) throw new Exception("Tên đăng nhập hoặc Email đã được sử dụng!");

            var newUser = new User
            {
                Uid = Guid.NewGuid().ToString(),
                DisplayName = model.Displayname,
                Username = model.Username,
                Email = model.Email,
                Phone = model.PhoneNumber,
                CreatorPoint = model.CreatorPoint,
                Password = model.Password,
                RoleId = model.RoleID,
                Status = model.Status
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return newUser;
        }

        public async Task<User> UpdateUserAsync(UpdateUserDto model)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Uid == model.Uid);
            if (existingUser == null) throw new Exception("Không tìm thấy tài khoản cần chỉnh sửa!");

            bool emailConflict = await _context.Users.AnyAsync(u => u.Email == model.Email && u.Uid != model.Uid);
            if (emailConflict) throw new Exception("Email này đã được tài khoản khác sử dụng!");

            existingUser.DisplayName = model.Displayname;
            existingUser.Email = model.Email;
            existingUser.RoleId = model.RoleID;
            existingUser.Status = model.Status;

            if (!string.IsNullOrEmpty(model.NewPassword))
                existingUser.Password = model.NewPassword;

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();
            return existingUser;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}