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
                var result = await _matchService.CalculateScoreAsync(request.User, request.House);
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
            //if (user.SubscriptionTier < 3)
            //{
            //    return BadRequest(new { message = "此為 VIP 專屬功能，請升級「尊榮 AI 秘書」方案解鎖！" });
            //}

            try
            {
                var activeHouses = await _context.Rent_Houses
                    .Where(h => h.RentalStatus == "available" && h.IsVisible == true)
                    .ToListAsync();

                if (!activeHouses.Any())
                {
                    return Ok(new List<HouseMatchResultDto>());
                }

                var semaphore = new SemaphoreSlim(2);

                var matchTasks = activeHouses.Select(async house =>
                {
                    await semaphore.WaitAsync();

                    try
                    {
                        var aiResult = await _matchService.CalculateScoreAsync(user, house);

                        return new HouseMatchResultDto
                        {
                            HouseId = house.Id,
                            Name = house.Name,
                            RentPrice = house.RentPrice,
                            HouseType = house.HouseType,
                            Score = aiResult.Score,
                            Basis = aiResult.Basis,
                            Risk = aiResult.Risk,
                            Suggestion = aiResult.Suggestion
                        };
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                //var matchTasks = activeHouses.Select(async house =>
                //{

                //    var aiResult = await _matchService.CalculateScoreAsync(user, house);

                //    return new HouseMatchResultDto
                //    {
                //        HouseId = house.Id,
                //        Name = house.Name,
                //        RentPrice = house.RentPrice,
                //        HouseType = house.HouseType,
                //        Score = aiResult.Score,
                //        Reason = aiResult.Reason
                //    };
                //});

                var allResults = await Task.WhenAll(matchTasks);
                var sortedResults = allResults.OrderByDescending(r => r.Score).ToList();

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
