using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentApi.Models;
using RentApi.Models.DTO;
using RentApi.Data;
namespace RentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchController : ControllerBase
    {
        private readonly MatchService _matchService;

        private readonly AppDbContext _context;

        // 注入我們剛剛寫好的 MatchService 大腦
        public MatchController(MatchService matchService, AppDbContext context)
        {
            _matchService = matchService;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> PostMatch([FromBody] MatchRequest request)
        {
            if (request == null || request.User == null || request.House == null)
            {
                return BadRequest(new { message = "請求資料不可為空！" });
            }

            try
            {
                
                var singleHouseList = new List<object> { request.House };

               
                var batchResults = await _matchService.CalculateBatchScoresAsync(request.User, singleHouseList);

                
                var result = batchResults.FirstOrDefault();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("match-all")]
        public async Task<IActionResult> MatchAllHouses([FromBody] UserProfileDto user)
        {
            // if (user.SubscriptionTier < 3)
            // {
            //     return BadRequest(new { message = "此為 VIP 專屬功能，請升級「尊榮 AI 秘書」方案解鎖！" });
            // }

            try
            {
                // 1. 撈出所有可出租的房子
                var activeHouses = await _context.Rent_Houses
                    .Where(h => h.RentalStatus == "available" && h.IsVisible == true)
                    .ToListAsync();

                if (!activeHouses.Any())
                {
                    return Ok(new List<HouseMatchResultDto>());
                }

                var aiResults = await _matchService.CalculateBatchScoresAsync(user, activeHouses);

               
                var houseLookup = activeHouses.ToDictionary(h => h.Id);
                var aiResult = await _matchService.CalculateScoreAsync(user, house);
               
                var sortedResults = aiResults
                    .Where(ai => houseLookup.ContainsKey(ai.HouseId)) 
                    .Select(ai =>
                    {
                        var house = houseLookup[ai.HouseId];
                        return new HouseMatchResultDto
                        {
                            HouseId = house.Id,
                            Name = house.Name,
                            RentPrice = house.RentPrice,
                            HouseType = house.HouseType,
                            Score = ai.Score,
                            Basis = ai.Basis,
                            Risk = ai.Risk,
                            Suggestion = ai.Suggestion
                        };
                    })
                    .OrderByDescending(r => r.Score) 
                    .ToList();

                return Ok(sortedResults);
            }
            catch (HttpRequestException ex)
            {
                
                return StatusCode(503, new
                {
                    message = "AI 目前連線異常，請稍後再試！",
                    details = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "伺服器內部發生錯誤，工程師正在搶修中！",
                    details = ex.Message
                });
            }
        }
        }
        }
        }
    }
}
