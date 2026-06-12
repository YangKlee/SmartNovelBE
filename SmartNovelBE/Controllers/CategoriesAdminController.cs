using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNovelBE.Models;
using SmartNovelBE.Services;
using System;
using System.Threading.Tasks;

namespace SmartNovelBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [AllowAnonymous] 
    public class CategoriesAdminController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesAdminController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories([FromQuery] string? keyword, [FromQuery] string? status, [FromQuery] int page = 1)
        {
            try
            {
                var result = await _categoryService.GetCategoriesAsync(keyword, status, page);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi tải danh sách!", error = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCategory([FromBody] Category model)
        {
            try
            {
                var newCategory = await _categoryService.CreateCategoryAsync(model);
                return Ok(new { message = "Thêm thể loại thành công!", data = newCategory });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCategory(string id, [FromBody] Category model)
        {
            if (id != model.CategoryId) return BadRequest(new { message = "ID không khớp!" });

            try
            {
                var updatedCategory = await _categoryService.UpdateCategoryAsync(model);
                return Ok(new { message = "Cập nhật thành công!", data = updatedCategory });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            try
            {
                var isDeleted = await _categoryService.DeleteCategoryAsync(id);
                if (!isDeleted) return NotFound(new { message = "Không tìm thấy thể loại cần xóa!" });

                return Ok(new { message = "Xóa thể loại thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
