using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentApi.Data;
using RentApi.Models.DTO;
using RentApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace RentApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase {
        private readonly UserService _service;
        public UserController(UserService service) {
            _service = service;
        }
        // 取得使用者列表
        [HttpGet]
        public async Task<IActionResult> Get() {
            var res = await _service.GetAllAsync();
            if (res == null)
                return NoContent();
            return Ok(res);
        }

        //  取得使用者個人資料
        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetUserAsync(int userId) {
            var result = await _service.GetProfileAsync(userId);
            if (result == null)
                return NotFound();
            return Ok(result);
        }
        // 更新使用者個人資料
        [HttpPut("profile/update")]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateProfileDto dto) {
            var result = await _service.UpdateProfileAsync(dto);
            if (result == null)
                return NotFound("User not found");
            return Ok(result);
        }
        [HttpPut("/status/{userid}")]
        public async Task<IActionResult> ChangeStatus(int userid) {
            var result = await _service.ChangeStatusAsync(userid);
            if (!result)
                return NotFound("User not found");
            return Ok();
        }

        // 取得出租人公開個人檔案 (無論有沒有登入都能看)
        [AllowAnonymous]
        [HttpGet("public-profile/{accountId}")]
        public async Task<IActionResult> GetPublicProfileAsync(int accountId)
        {
            var result = await _service.GetPublicProfileByAccountIdAsync(accountId);

            if (result == null)
                return NotFound("找不到該會員的公開檔案");

            return Ok(result);
        }
    }
}
