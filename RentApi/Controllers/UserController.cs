using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentApi.Data;
using RentApi.Models;
using RentApi.Models.DTO;
using RentApi.Services;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

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
        [HttpPut("status/{userid}")]
        public async Task<IActionResult> ChangeStatus(int userid) {
            var result = await _service.ChangeStatusAsync(userid);
            if (!result)
                return NotFound("User not found");
            return Ok();
        }

        [HttpPut("delete/{userid}")]
        public async Task<IActionResult> DeleteUser(int userid) {
            var result = await _service.DeleteUserAsync(userid);
            if (!result) {
                return NotFound("User not found");
            }
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

        [HttpGet("account-setting/{userId}")]
        public async Task<IActionResult> GetAccountSetting(int userId) {
            var result = await _service.GetAccountSettingAsync(userId);
            if (result == null)
                return NotFound("Account settings not found");
            return Ok(result);
        }
        [HttpPut("update-email/{userId}")]
        public async Task<IActionResult> UpdateEmail(int userId, [FromBody] UpdateEmailDto dto) {
            var result = await _service.ChangeEmailAsync(userId, dto.Email);
            if(!result) {
                return NotFound("User not found");
            }
            return Ok();
        }
        [HttpPut("update-pwd/{userId}")]
        public async Task<IActionResult> UpdatePwd(int userId, [FromBody] UpdatePwdDto dto) {
            var result = await _service.ChangePwdAsync(userId, dto.Pwd);
            if (!result) {
                return NotFound("User not found");
            }
            return Ok();
        }
        [HttpGet("get-notification/{userId}")]
        public async Task<IActionResult> GetNotification(int userId) {
            var setting = await _service.GetSetting(userId);
            if (setting == null)
                return NotFound();
            return Ok(setting);
        }

        [HttpPut("update-notification/{userId}")]
        public async Task<IActionResult> UpdateNotification(int userId,[FromBody] NotificationSettingDto request) {
            var setting = await _service.SaveSetting(userId,request);
            if (setting == null) {
                return BadRequest();
            }
            return Ok(new {
                message = "更新成功",
                data = setting
            });
        }
    }
        [HttpPut("upgrade/{userId}/{tier}")]
        public async Task<IActionResult> UpgradeUserTier(int userId, int tier)
        {
            
            var result = await _service.UpgradeUserTierAsync(userId, tier);

            if (!result)
            {
                
                return BadRequest(new { message = "金流授權成功，但系統目前連線擁擠，您的 VIP 權限將於 5 分鐘內自動開通！" });
            }

            return Ok(new { message = "資料庫升級成功！", newTier = tier });
        }

    }
}
