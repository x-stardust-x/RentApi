using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RentApi.Interfaces;
using RentApi.Models.DTO;

namespace RentApi.Controllers
{
    // 標記這是 API 控制器，以及設定路由為 api/RentalMatching
    [Route("api/[controller]")]
    [ApiController]
    public class RentalMatchingController : ControllerBase
    {
        private readonly IRentalMatchingService _rentalMatchingService;

        // 透過建構子注入剛剛寫好的 Service
        public RentalMatchingController(IRentalMatchingService rentalMatchingService)
        {
            _rentalMatchingService = rentalMatchingService;
        }

        // 端點一：GET api/Rental
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Match_HouseDto>>> GetAllAsync()
        {
            try
            {
                var rentals = await _rentalMatchingService.GetRentalAsync();

                // 回傳 HTTP 200 OK，並把資料裝在裡面送出去
                return Ok(rentals);
            }
            catch (Exception ex)
            {
                // 如果後端報錯，回傳 HTTP 500 錯誤訊息，方便除錯
                return StatusCode(500, $"伺服器發生錯誤:{ex.Message}");
            }
        }

        // 端點二：GET api/Rental/{id} ( 根據 ID 取得單一房子詳情 )
        [HttpGet("{id}")]
        public async Task<ActionResult<Match_HouseDto>> GetRentalById(int id)
        {
            try
            {
                var rental = await _rentalMatchingService.GetRentalByAsync(id);

                if (rental == null)
                {
                    // 如果找不到這間房，回傳 HTTP 404 Not Found
                    return NotFound($"找不到 ID 為 {id} 的房屋資料");
                }

                return Ok(rental);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"伺服器發生錯誤：{ex.Message}");
            }
        }


    }
}
