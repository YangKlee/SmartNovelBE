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
    [AllowAnonymous]
    public class UsersAdminController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersAdminController(IUserService userService)
        {
            _userService = userService;
        }


        //ADMIN QUAN LI USER
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetUsers([FromQuery] string? keyword, [FromQuery] string? role, [FromQuery] string? status, [FromQuery] int page = 1)
        {
            try
            {
                var result = await _userService.GetUsersAsync(keyword, role, status, page);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi tải danh sách!", error = ex.Message });
            }
        }

        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUsers([FromBody] CreateUserDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var newUser = await _userService.CreateUserAsync(model);
                return Ok(new { message = "Thêm người dùng thành công!", data = newUser });
            }
            catch (Exception ex)
            {
           
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPut("update/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateUsers(string id, [FromBody] UpdateUserDto model)
        {
            if (id != model.Uid) return BadRequest(new { message = "ID không khớp với dữ liệu gửi lên" });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var updatedUser = await _userService.UpdateUserAsync(model);
                return Ok(new { message = "Cập nhật thành công", data = updatedUser });
            }
            catch (Exception ex)
            {
     
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteUsers(string id)
        {
            try
            {
                var isDeleted = await _userService.DeleteUserAsync(id);
                if (!isDeleted) return NotFound(new { message = "Không tìm thấy tài khoản cần xóa" });

                return Ok(new { message = "Xóa tài khoản thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi thực hiện xóa dữ liệu", error = ex.Message });
            }
        }
    }
}
