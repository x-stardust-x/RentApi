using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentApi.Data;
using RentApi.Models.DTO;

namespace RentApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase {
        private readonly AppDbContext _db;
        public UserController(AppDbContext db) {
            _db = db;
        }
        [HttpGet("profile/{userId}")]
        public async Task<IActionResult?> GetUserProfileAsync(int userId) {
            var result = await (
                from u in _db.User
                join d in _db.District on u.DistrictId equals d.Id
                join c in _db.City on d.CityId equals c.Id
                join h in _db.User_Habit on u.Id equals h.UserId into habitGroup
                from h in habitGroup.DefaultIfEmpty()

                where u.Id == userId

                select new UserProfileDto {
                    Id = u.Id,

                    RealName = u.RealName,
                    EnglishName = u.EnglishName,
                    Avatar = u.Avatar,
                    Phone = u.Phone,
                    Address = u.Address,
                    Bio = u.Bio,

                    Rating = u.Rating,
                    ReviewCount = u.ReviewCount,

                    CityName = c.CityName,
                    DistrictName = d.DistrictName,
                    ZipCode = d.ZipCode,

                    SleepTime = h != null ? h.SleepTime : 0,
                    WakeTime = h != null ? h.WakeTime : 0,
                    CleanLevel = h != null ? h.CleanLevel : 0,
                    NoiseTolerance = h != null ? h.NoiseTolerance : 0,
                    Pet = h.Pet,
                    Smoke = h.Smoke,
                    Interests = h != null ? h.Interests : null
                }
            ).FirstOrDefaultAsync();

            return Ok(result);
        }
        [HttpGet("GetUserHabbit/{userid}")]
        public IActionResult EditUser(int userId) {
            return BadRequest();
        }
    }

}
