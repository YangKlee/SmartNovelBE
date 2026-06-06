using Microsoft.EntityFrameworkCore;
using SmartNovelBE.DTOs.AdminUser;
using SmartNovelBE.Models;
using SmartNovelBE.Services;
using System.Net.Quic;

namespace SmartNovel.Services
{
    public class UserService : IUserService
    {
        private readonly SmartTruyenDbContext _context;

        public UserService(SmartTruyenDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetUsersAsync(string? keyword, string? role, string? status, int page, int pageSize = 10)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(u => u.DisplayName.Contains(keyword) || u.Username.Contains(keyword) || u.Email.Contains(keyword));

            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.RoleId == role);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(u => u.Status == status);
            int skip = (page - 1) * pageSize;
            int totalUsers = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            var userDtos = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(u => new UserListDto
                {
                    Uid = u.Uid,
                    Username = u.Username,
                    DisplayName = u.DisplayName,
                    Email = u.Email,
                    Phone = u.Phone,
                    RoleId = u.RoleId,
                    Status = u.Status,
                    CreatorPoint = u.CreatorPoint,
                    Birthday = u.Birthday
                })
                .ToListAsync();

            return new
            {
                Data = userDtos,
                TotalPages = totalPages,
                TotalUsers = totalUsers,
                CurrentPage = page
            };
        }

        public async Task<User> CreateUserAsync(CreateUserDto model)
        {
            bool isExists = await _context.Users.AnyAsync(u => u.Username == model.Username || u.Email == model.Email);
            if (isExists) throw new Exception("Tên đăng nhập hoặc Email đã được sử dụng!");
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
            var newUser = new User
            {
                Uid = Guid.NewGuid().ToString(),
                DisplayName = model.DisplayName,
                Username = model.Username,
                Email = model.Email,
                Phone = model.Phone,
                CreatorPoint = model.CreatorPoint,
                Password = hashedPassword,
                RoleId = model.RoleId,
                Status = model.Status
            };

            try
            {
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return newUser;
        }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                string exactError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                throw new Exception("Lỗi Database: " + exactError);
            }
        }

        public async Task<User> UpdateUserAsync(UpdateUserDto model)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Uid == model.Uid);
            if (existingUser == null) throw new Exception("Không tìm thấy tài khoản cần chỉnh sửa!");

            bool emailConflict = await _context.Users.AnyAsync(u => u.Email == model.Email && u.Uid != model.Uid);
            if (emailConflict) throw new Exception("Email này đã được tài khoản khác sử dụng!");

            existingUser.DisplayName = model.DisplayName;
            existingUser.Email = model.Email;
            existingUser.RoleId = model.RoleId;
            existingUser.Status = model.Status;
            existingUser.Phone = model.Phone;
            existingUser.CreatorPoint = model.CreatorPoint;
            if (!string.IsNullOrEmpty(model.Password))
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

            if (!string.IsNullOrEmpty(model.NewPassword))
                existingUser.Password = model.NewPassword;

            try
            {
            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();
            return existingUser;
        }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                string exactError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                throw new Exception("Lỗi Database: " + exactError);
            }
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