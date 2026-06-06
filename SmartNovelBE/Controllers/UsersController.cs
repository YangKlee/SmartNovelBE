using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartNovelBE.DTOs.AdminUser;
using SmartNovelBE.Models;
using SmartNovelBE.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartNovelBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly SmartTruyenDbContext _context;
        private readonly JwtServices _jwtServices;

        public UsersController(SmartTruyenDbContext context)
        {
            _context = context;

        }



        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, User user)
        {
            if (id != user.Uid)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UserExists(user.Uid))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetUser", new { id = user.Uid }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Users/authors
        [HttpGet("authors")]
        [AllowAnonymous] // Cho phép Angular gọi công khai ngoài bộ lọc mà không cần login
        public async Task<IActionResult> GetAuthorsOnly()
        {
            try
            {
                // Lọc ra các User có RoleID là '3' (Tác giả) và đang hoạt động (ACTIVE)
                var authors = await _context.Users
                    .Where(u => u.RoleId == "3" && u.Status == "ACTIVE")
                    .Select(u => new
                    {
                        Uid = u.Uid,
                        DisplayName = u.DisplayName
                    })
                    .ToListAsync();

                return Ok(authors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Uid == id);
        }
    

        //ADMIN QUAN LY USER
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetUsers(string? keyword, string? role, string? status, int page = 1)
        {
            int pageSize = 10;
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

            return Ok(new { data = users, currentPage = page, totalPages = totalPages, totalRecords = totalUsers });
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreateUsers([FromBody] CreateUserDto model)
        {
            bool isExists = await _context.Users.AnyAsync(u => u.Username == model.Username || u.Email == model.Email);
            if (isExists) return BadRequest(new { message = "Tên đăng nhập hoặc Email đã tồn tại!" });

            var newUser = new User
            {
                Uid = Guid.NewGuid().ToString(),
                DisplayName = model.Displayname,
                Username = model.Username,
                Email = model.Email,
                Phone = model.PhoneNumber,
                CreatorPoint = model.CreatorPoint,
                Password = model.Password, // Lưu ý: Cần mã hóa (Hash) mật khẩu trước khi lưu ở thực tế
                RoleId = model.RoleID,
                Status = model.Status
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm người dùng thành công!", data = newUser });
        }


        [HttpPut("update/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateUsers(string id, [FromBody] UpdateUserDto model)
        {
            if (id != model.Uid) return BadRequest(new { message = "ID không khớp với dữ liệu gửi lên!" });

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Uid == id);
            if (existingUser == null) return NotFound(new { message = "Không tìm thấy tài khoản cần chỉnh sửa!" });

            bool emailConflict = await _context.Users.AnyAsync(u => u.Email == model.Email && u.Uid != id);
            if (emailConflict) return BadRequest(new { message = "Email này đã được tài khoản khác sử dụng!" });

            existingUser.DisplayName = model.Displayname;
            existingUser.Email = model.Email;
            existingUser.RoleId = model.RoleID;
            existingUser.Status = model.Status;

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                existingUser.Password = model.NewPassword;
            }

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thành công!", data = existingUser });
        }

        [HttpDelete("delete/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteUsers(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == id);
            if (user == null) return NotFound(new { message = "Không tìm thấy tài khoản cần xóa!" });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa tài khoản thành công!" });
        }
    }
}
