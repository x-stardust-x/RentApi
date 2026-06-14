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

        public async Task<List<UserDto>> GetAllAsync() {
            var result = from u in _db.User
                         join a in _db.Account
                         on u.AccountId equals a.Id into gj
                         from a in gj.DefaultIfEmpty()
                         select new UserDto {
                             Id = u.Id,
                             AccountId = u.AccountId,
                             RealName = u.RealName,
                             EnglishName = u.EnglishName,
                             Avatar = u.Avatar,
                             Address = u.Address,
                             Phone = u.Phone,
                             LineId = u.LineId,
                             Bio = u.Bio,
                             Rating = u.Rating,
                             ReviewCount = u.ReviewCount,
                             CreateAt = u.CreateAt,

                             Age = a != null ? a.Age : 0,
                             Identity = a != null ? a.Identity : Identity.young,
                             Status = a != null && a.Status,
                             IsDelete = a != null && a.IsDelete,
                             LastLoginAt = a != null ? a.LastLoginAt : null
                         };

            return await result.ToListAsync();
        }

        public async Task<UserProfileDto?> GetProfileAsync(int userId) {
            return await(
                from u in _db.User
                join d in _db.Location_Districts on u.DistrictId equals d.DistrictId
                join h in _db.User_Habit on u.Id equals h.UserId into habitGroup
                from h in habitGroup.DefaultIfEmpty()

                where u.Id == userId

                select new UserProfileDto {
                    Id = u.Id,

                    RealName = u.RealName,
                    EnglishName = u.EnglishName,
                    Avatar = u.Avatar,
                    Phone = u.Phone,
                    LineId = u.LineId,
                    Address = u.Address,
                    Bio = u.Bio,

                    Rating = u.Rating,
                    ReviewCount = u.ReviewCount,

                    DistrictId = u.DistrictId,
                    CityName = d.CityName,
                    DistrictName = d.DistrictName,
                    ZipCode = d.ZipCode,

                    SleepTime = h != null && h.SleepTime != null ? h.SleepTime.Value : TimeOnly.MinValue,
                    WakeTime = h != null && h.WakeTime != null ? h.WakeTime.Value : TimeOnly.MinValue,
                    CleanLevel = h != null ? h.CleanLevel : 0,
                    NoiseTolerance = h != null ? h.NoiseTolerance : 0,
                    Pet = h != null && (h.Pet ?? false),
                    Smoke = h != null && (h.Smoke ?? false),
                    Interests = h != null ? h.Interests ?? "" : ""
                }
            ).FirstOrDefaultAsync();
        }
        public async Task<UpdateProfileDto?> UpdateProfileAsync(UpdateProfileDto dto)
        {
            var res = await _db.User.FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (res == null)
            {
                return null;
            }

            res.DistrictId = dto.DistrictId;
            res.RealName = dto.RealName;
            res.EnglishName = dto.EnglishName;
            res.Avatar = dto.Avatar;
            res.Phone = dto.Phone;
            res.LineId = dto.LineId;
            res.Address = dto.Address;
            res.Bio = dto.Bio;

            var res_habit = await _db.User_Habit
                .FirstOrDefaultAsync(x => x.UserId == dto.Id);

            if (res_habit == null)
            {
                res_habit = new User_Habit
                {
                    UserId = dto.Id
                };

                _db.User_Habit.Add(res_habit);
            }

            res_habit.SleepTime = dto.SleepTime;
            res_habit.WakeTime = dto.WakeTime;
            res_habit.CleanLevel = dto.CleanLevel;
            res_habit.NoiseTolerance = dto.NoiseTolerance;
            res_habit.Pet = dto.Pet;
            res_habit.Smoke = dto.Smoke;
            res_habit.Interests = dto.Interests ?? "";

            await _db.SaveChangesAsync();

            return dto;
        }

        public Task<bool> ChangeStatusAsync(int userid) {
            var res = _db.Account.FirstOrDefault(x => x.Id == userid);
            if (res == null) {
                return Task.FromResult(false);
            }
            res.Status = !res.Status;
            _db.SaveChanges();
            return Task.FromResult(true);
        }

        public Task<bool> DeleteUserAsync(int userid) {
            var res = _db.Account.FirstOrDefault(x => x.Id == userid);
            if(res == null) {
                return Task.FromResult(false);
            }
            res.IsDelete = true;
            _db.SaveChanges();
            return Task.FromResult(true);
        }

        public async Task<LessorPublicProfileDto?> GetPublicProfileByAccountIdAsync(int accountId)
        {
            // 1. 撈取使用者基本資料 (將 _context 改為 _db，Users 改為 User)
            var user = await _db.User.FirstOrDefaultAsync(u => u.AccountId == accountId);
            if (user == null) return null;

            // 2. 撈取生活習慣 (因為沒有導覽屬性，改用獨立查詢對接 UserId)
            var habit = await _db.User_Habit.FirstOrDefaultAsync(h => h.UserId == user.Id);

            // 3. 撈取該出租人目前上架中的房屋列表 (根據你的 Network 截圖，DbSet 名稱應為 RentHouse)
            // 💡 備註：如果這行報錯，請去查看你們 AppDbContext 裡面的房屋表叫什麼名字
            var activeHousesRaw = await _db.Rent_Houses
    .AsNoTracking()
    .Where(h => h.AccountId == accountId && h.Status == 1)
    .Select(h => new
    {
        h.Id,
        h.Name,
        h.RentPrice
    })
    .ToListAsync();

            var houseIds = activeHousesRaw.Select(h => h.Id).ToList();

            var houseImageMap = await _db.House_Images
                .AsNoTracking()
                .Where(img => houseIds.Contains(img.HouseId))
                .OrderByDescending(img => img.IsCover)
                .ThenBy(img => img.Id)
                .GroupBy(img => img.HouseId)
                .Select(g => new
                {
                    HouseId = g.Key,
                    Url = g.Select(x => x.Url).FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.HouseId, x => x.Url ?? "");

            var activeHouses = activeHousesRaw
                .Select(h => new LessorPublicProfileDto.HouseDto
                {
                    Id = h.Id,
                    Name = h.Name ?? "未命名房源",
                    RentPrice = h.RentPrice,
                    MainImageUrl = houseImageMap.ContainsKey(h.Id)
                        ? houseImageMap[h.Id]
                        : "",
                    DisplayType = "room"
                })
                .ToList();


            var activeProductsRaw = await _db.Rent_Products
                .AsNoTracking()
                .Where(p => p.AccountId == accountId && p.Status == 1)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Category,
                    p.Price,
                    p.PriceUnit
                })
                .ToListAsync();

            var productIds = activeProductsRaw.Select(p => p.Id).ToList();

            var productImageMap = await _db.Product_Image
                .AsNoTracking()
                .Where(img => productIds.Contains(img.ProductId))
                .OrderByDescending(img => img.IsCover)
                .ThenBy(img => img.Id)
                .GroupBy(img => img.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Url = g.Select(x => x.Url).FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.ProductId, x => x.Url ?? "");

            var activeProducts = activeProductsRaw
                .Select(p => new LessorPublicProfileDto.ProductDto
                {
                    Id = p.Id,
                    Name = p.Name ?? "未命名資源",
                    Category = p.Category ?? "",
                    Price = p.Price ?? 0,
                    PriceUnit = p.PriceUnit ?? "次",
                    MainImageUrl = productImageMap.ContainsKey(p.Id)
                        ? productImageMap[p.Id]
                        : "",
                    DisplayType = "product"
                })
                .ToList();

            // 4. 計算加入年資 (防呆：若 User 表沒 CreatedAt 欄位可先寫死 1)
            int joinYears = 1;

            // 5. 處理 TimeOnly 轉 int 小時問題
            int sleepHour = habit?.SleepTime?.Hour ?? 23; // 🟢 改用 .Hour 取得純數字小時 (0~23)
            int wakeHour = habit?.WakeTime?.Hour ?? 7;    // 🟢 改用 .Hour

            // 6. 打包成 DTO 回傳 (對應你 User 表的真實欄位名)
            return new LessorPublicProfileDto
            {
                AccountId = accountId,
                RealName = user.RealName ?? "",
                Username = user.EnglishName ?? user.RealName ?? "",
                Phone = user.Phone ?? "",
                LineId = user.LineId ?? "",
                AvatarUrl = user.Avatar ?? "assets/default-avatar.png", // 你的欄位叫 Avatar
                BannerUrl = "assets/default-banner.jpg",
                IsVerified = true,
                Identity = 0,
                ProfileTitle = "共享居住夥伴",
                ProfileSubTitle = "",
                Bio = user.Bio ?? "尚未填寫自我介紹",
                //Rating = user.Rating,         // 使用你 User 表既有的 Rating
                //ReviewCount = user.ReviewCount, // 使用你 User 表既有的 ReviewCount
                JoinYears = joinYears,

                // 生活習慣
                Smoke = habit?.Smoke ?? false,
                SleepTime = sleepHour,
                WakeTime = wakeHour,
                Pet = habit?.Pet ?? false,
                NoiseTolerance = habit?.NoiseTolerance ?? 3,
                Interests = habit?.Interests ?? "",

                // 巢狀清單
                ActiveHouses = activeHouses,
                ActiveProducts = activeProducts,
                Reviews = new List<LessorPublicProfileDto.ReviewDto>()
            };
        }

        public async Task<object> GetAccountSettingAsync(int userId) {
            var res = await (
                from u in _db.User
                join a in _db.Account on u.AccountId equals a.Id
                where u.Id == userId
                select new {
                    email = a.Email,
                    phone = u.Phone,
                    lineId = u.LineId,
                }
            ).FirstOrDefaultAsync();

            return res;
        }

        public Task<bool> ChangeEmailAsync(int userid, string email) {
            var res = _db.Account.FirstOrDefault(x => x.Id == userid);
            if (res == null) {
                return Task.FromResult(false);
            }
            res.Email = email;
            _db.SaveChanges();
            return Task.FromResult(true);
        }
        public Task<bool> ChangePwdAsync(int userid, string pwd) {
            var res = _db.Account.FirstOrDefault(x => x.Id == userid);
            if (res == null) {
                return Task.FromResult(false);
            }
            string hashedPwd = PasswordHelper.Hash(pwd);
            res.Pwd = hashedPwd;
            _db.SaveChanges();
            return Task.FromResult(true);
        }
    }
}