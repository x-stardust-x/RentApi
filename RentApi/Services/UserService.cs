using Microsoft.EntityFrameworkCore;
using RentApi.Data;
using RentApi.Models;
using RentApi.Models.DTO;

namespace RentApi.Services {
    public class UserService {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db) {
            _db = db;
        }

        public async Task<List<User>> GetAllAsync() {
            return await _db.User.ToListAsync();
        }

        public async Task<UserProfileDto?> GetProfileAsync(int userId) {
            return await(
                from u in _db.User
                join d in _db.Location_District on u.DistrictId equals d.DistrictId
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

                    DistrictId = u.DistrictId,
                    CityName = d.CityName,
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
        }
        public async Task<UpdateProfileDto?> UpdateProfileAsync(UpdateProfileDto dto) {
            var res = await _db.User.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if(res == null) {
                return null;
            }
            var res_habit = await _db.User_Habit.FirstOrDefaultAsync(x => x.UserId == dto.Id);
            if(res_habit == null) {
                return null;
            }
            res.DistrictId = dto.DistrictId;
            res.RealName = dto.RealName;
            res.EnglishName = dto.EnglishName;
            res.Avatar = dto.Avatar;
            res.Address = dto.Address;
            res.Bio = dto.Bio;

            res_habit.SleepTime = dto.SleepTime;
            res_habit.WakeTime = dto.WakeTime;
            res_habit.CleanLevel = dto.CleanLevel;
            res_habit.NoiseTolerance = dto.NoiseTolerance;
            res_habit.Pet = dto.Pet;
            res_habit.Smoke = dto.Smoke;
            res_habit.Interests = dto.Interests;

            await _db.SaveChangesAsync();

            return dto;
        }
    }
}