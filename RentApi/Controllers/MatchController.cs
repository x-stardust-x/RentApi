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

            
            var result = await _matchService.CalculateScoreAsync(request.User, request.House);

            return Ok(result);
        }

        [HttpPost("match-all")]
        public async Task<IActionResult> MatchAllHouses([FromBody] UserProfileDto user)
        {
            
            var activeHouses = await _context.Rent_Houses
                .Where(h => h.RentalStatus == "available" && h.IsVisible == true)
                .ToListAsync();

            if (!activeHouses.Any()) {
                return Ok(new List<HouseMatchResultDto>());
            }


            var matchTasks = activeHouses.Select(async house =>
            {
               
                var aiResult = await _matchService.CalculateScoreAsync(user, house);

                return new HouseMatchResultDto
                {
                    HouseId = house.Id,
                    Name = house.Name,
                    RentPrice = house.RentPrice,
                    HouseType = house.HouseType,
                    Score = aiResult.Score,
                    Reason = aiResult.Reason
                };
            });

            
            var allResults = await Task.WhenAll(matchTasks);

            
            var sortedResults = allResults.OrderByDescending(r => r.Score).ToList();

            return Ok(sortedResults);
        }
    }
}
