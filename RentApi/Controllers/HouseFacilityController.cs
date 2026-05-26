using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RentApi.Models.DTO;
using RentApi.Services;

namespace RentApi.Controllers
{
    [Route("api/HouseFacility")]
    [ApiController]
    public class HouseFacilityController : ControllerBase
    {
        private readonly HouseFacilityService _houseService;

        public HouseFacilityController(HouseFacilityService houseService) => _houseService = houseService;

        // GET: api/HouseFacility
        [HttpGet]
        public async Task<IActionResult> GetAllFacilities()
        {
            var result = await _houseService.GetAllFacilitiesAsync();
            return Ok(result);
        }

        // ===================================================================
        // GET: api/HouseFacility/9
        // 專門供前台詳情頁呼叫，用來動態撈取這間房子到底有哪些設施
        // ===================================================================
        [HttpGet("{houseId}")]
        public async Task<IActionResult> GetHouseFacilities(int houseId)
        {
            try
            {
                // 當上方的 Service 檔案補上 GetHouseFacilitiesAsync 方法後，這裡就不會再噴紅字了！
                var result = await _houseService.GetHouseFacilitiesAsync(houseId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                // 如果在資料庫找不到這間房子，回傳 404 錯誤
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // 其他非預期錯誤，回傳 400 錯誤
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/HouseFacility/save-house
        [HttpPost("save-house")]
        public async Task<IActionResult> SaveHouseFacilities([FromBody] HouseFacilityUpdateDto dto)
        {
            try
            {
                await _houseService.UpdateHouseFacilitiesAsync(dto);
                return Ok(new { message = "房屋設施更新成功！" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}