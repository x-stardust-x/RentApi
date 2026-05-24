using Microsoft.EntityFrameworkCore;
using RentApi.Data;
using RentApi.Interfaces;
using RentApi.Models;
using RentApi.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourProjectNamespace.Dtos;

namespace RentApi.Services
{
    public class RentalMatchingService : IRentalMatchingService
    {
        private readonly AppDbContext _context;

        public RentalMatchingService(AppDbContext context)
        {
            _context = context;
        }

        // 1. 抓取所有【上架中】的房子 (回傳清單)
        public async Task<IEnumerable<Match_HouseDto>> GetRentalAsync()
        {
            var query = from house in _context.Rent_Houses

                        join rule in _context.HouseRules on house.Id equals rule.HouseId into rules
                        from rule in rules.DefaultIfEmpty()

                        join account in _context.Account on house.AccountId equals account.Id into accounts
                        from account in accounts.DefaultIfEmpty()

                        join user in _context.User on (account != null ? account.Id : -1) equals user.AccountId into users
                        from user in users.DefaultIfEmpty()

                        join loc in _context.Location_Districts on house.DistrictId equals loc.DistrictId into locations
                        from loc in locations.DefaultIfEmpty()

                        where house.Status == 1
                        select new Match_HouseDto
                        {
                            Id = house.Id,
                            AccountId = house.AccountId,
                            DistrictId = house.DistrictId,
                            Name = house.Name,
                            Address = house.Address,
                            Description = house.Description,
                            RentPrice = house.RentPrice,
                            IncludeUtilities = house.IncludeUtilities,
                            IncludeWifi = house.IncludeWifi,
                            IncludeManagementFee = house.IncludeManagementFee,
                            AreaSize = house.AreaSize,
                            LeaseTerm = house.LeaseTerm,
                            FloorInfo = house.FloorInfo,
                            HouseType = house.HouseType,
                            ViewCount = house.ViewCount,
                            Status = house.Status,

                            CityName = loc != null ? loc.CityName : "未知縣市",
                            DistrictName = loc != null ? loc.DistrictName : "未知區域",
                            ZipCode = loc != null ? loc.ZipCode.ToString() : "",

                            HouseId = rule != null ? rule.HouseId : null,
                            SleepTime = rule != null ? (TimeOnly?)rule.SleepTime : null,
                            WakeTime = rule != null ? (TimeOnly?)rule.WakeTime : null,
                            CleanLevel = rule != null ? (int?)rule.CleanLevel : null,
                            NoiseTolerance = rule != null ? (int?)rule.NoiseTolerance : null,
                            Pet = rule != null ? rule.Pet : null,
                            Smoke = rule != null ? rule.Smoke : null,
                            AdvancedRules = rule != null ? rule.AdvancedRules : null,

                            RealName = user != null ? user.RealName : "未知提供者"
                        };

            return await query.ToListAsync();
        }

        // 2. 根據 ID 抓取單一房屋【詳情】
        public async Task<Match_HouseDto?> GetRentalByAsync(int id)
        {
            var ruleCount = await _context.HouseRules.CountAsync(r => r.HouseId == id);
            Console.WriteLine($"除錯：資料庫中 HouseId 為 {id} 的規則筆數有 {ruleCount} 筆");

            var query = from house in _context.Rent_Houses

                        join rule in _context.HouseRules on house.Id equals rule.HouseId into rules
                        from rule in rules.DefaultIfEmpty()

                        join account in _context.Account on house.AccountId equals account.Id into accounts
                        from account in accounts.DefaultIfEmpty()

                        join user in _context.User on (account != null ? account.Id : -1) equals user.AccountId into users
                        from user in users.DefaultIfEmpty()

                        join loc in _context.Location_Districts on house.DistrictId equals loc.DistrictId into locations
                        from loc in locations.DefaultIfEmpty()

                        where house.Id == id
                        select new Match_HouseDto
                        {
                            Id = house.Id,
                            AccountId = house.AccountId,
                            DistrictId = house.DistrictId,
                            Name = house.Name,
                            Address = house.Address,
                            Description = house.Description,
                            RentPrice = house.RentPrice,
                            IncludeUtilities = house.IncludeUtilities,
                            IncludeWifi = house.IncludeWifi,
                            IncludeManagementFee = house.IncludeManagementFee,
                            AreaSize = house.AreaSize,
                            LeaseTerm = house.LeaseTerm,
                            FloorInfo = house.FloorInfo,
                            HouseType = house.HouseType,
                            ViewCount = house.ViewCount,
                            Status = house.Status,

                            CityName = loc != null ? loc.CityName : "未知縣市",
                            DistrictName = loc != null ? loc.DistrictName : "未知區域",
                            ZipCode = loc != null ? loc.ZipCode.ToString() : "",

                            UserName = account != null ? account.Username : "未知房東",
                            Bio = user != null ? user.Bio : "這位出租人還沒寫自我介紹",
                            Rating = user != null ? user.Rating : 0,
                            ReviewCount = user != null ? user.ReviewCount : 0,

                            HouseId = rule != null ? rule.HouseId : null,
                            SleepTime = rule != null ? rule.SleepTime : null,
                            WakeTime = rule != null ? rule.WakeTime : null,
                            CleanLevel = rule != null ? (int?)rule.CleanLevel : null,
                            NoiseTolerance = rule != null ? (int?)rule.NoiseTolerance : null,
                            Pet = rule != null ? rule.Pet : null,
                            Smoke = rule != null ? rule.Smoke : null,
                            AdvancedRules = rule != null ? rule.AdvancedRules : null,

                            ImageUrls = new List<string>(),
                            RealName = user != null ? user.RealName : "未知提供者"
                        };

            var houseResult = await query.FirstOrDefaultAsync();

            if (houseResult != null)
            {
                var images = await _context.House_Images
                                           .Where(img => img.HouseId == id)
                                           .Select(img => img.Url)
                                           .ToListAsync();

                houseResult.ImageUrls = images;
            }

            return houseResult;
        }

        // 3. 抓取所有工具技能
        public async Task<IEnumerable<Match_ProductDto>> GetProductAsync()
        {
            var query = from product in _context.Rent_Products

                        join account in _context.Account on product.AccountId equals account.Id into accounts
                        from account in accounts.DefaultIfEmpty()

                        join user in _context.User on (account != null ? account.Id : -1) equals user.AccountId into users
                        from user in users.DefaultIfEmpty()

                        select new Match_ProductDto
                        {
                            Id = product.Id,
                            AccountId = product.AccountId,
                            DistrictId = null,
                            Name = product.Name,
                            Category = product.Category,
                            Description = product.Description,
                            Price = product.Price,
                            PriceUnit = product.PriceUnit,
                            Deposit = product.Deposit,
                            IsOnline = product.IsOnline,
                            Quantity = product.Quantity,
                            CreatedAt = product.CreatedAt,
                            UpdatedAt = product.UpdatedAt,
                            OwnTool = product.OwnTool,
                            RequiredKnowledge = product.RequiredKnowledge,
                            Address = product.Address,
                            UsageRequirements = product.UsageRequirements,
                            UsageTerms = product.UsageTerms,

                            CityName = "",
                            DistrictName = "",

                            UserName = account != null ? account.Username : "未知提供者",
                            Bio = user != null ? user.Bio : "這位提供者還沒寫自我介紹",
                            Rating = user != null ? user.Rating : 0,
                            ReviewCount = user != null ? user.ReviewCount : 0,
                            RealName = user != null ? user.RealName : "未知提供者",
                        };

            return await query.ToListAsync();
        }

        // 4. 根據 ID 抓取單一工具技能詳情
        public async Task<Match_ProductDto?> GetProductByIdAsync(int id)
        {
            var query = from product in _context.Rent_Products

                        join account in _context.Account on product.AccountId equals account.Id into accounts
                        from account in accounts.DefaultIfEmpty()

                        join user in _context.User on (account != null ? account.Id : -1) equals user.AccountId into users
                        from user in users.DefaultIfEmpty()

                        where product.Id == id
                        select new Match_ProductDto
                        {
                            Id = product.Id,
                            AccountId = product.AccountId,
                            DistrictId = null,
                            Name = product.Name,
                            Category = product.Category,
                            Description = product.Description,
                            Price = product.Price,
                            PriceUnit = product.PriceUnit,
                            Deposit = product.Deposit,
                            IsOnline = product.IsOnline,
                            Quantity = product.Quantity,
                            OwnTool = product.OwnTool,
                            RequiredKnowledge = product.RequiredKnowledge,
                            Address = product.Address,
                            UsageRequirements = product.UsageRequirements,
                            UsageTerms = product.UsageTerms,

                            CityName = "",
                            DistrictName = "",

                            UserName = account != null ? account.Username : "未知提供者",
                            Bio = user != null ? user.Bio : "這位提供者還沒寫自我介紹",
                            Rating = user != null ? user.Rating : 0,
                            ReviewCount = user != null ? user.ReviewCount : 0,
                            RealName = user != null ? user.RealName : "未知提供者"
                        };

            return await query.FirstOrDefaultAsync();
        }

        // 🌟 核心修正：全新重構的綜合篩選器功能
        public async Task<IEnumerable<object>> SearchRentalsAsync(RentalFilterRequestDto request)
        {
            // 步驟 A：將前端傳入的英文城市代碼轉換為資料庫比對用的中文字串
            string? cityNameKeyword = null;
            if (!string.IsNullOrEmpty(request.City) && request.City != "all" && request.City.Trim() != "")
            {
                cityNameKeyword = request.City.ToLower() switch
                {
                    "taipei" => "台北",
                    "new-taipei" => "新北",
                    "taoyuan" => "桃園",
                    "taichung" => "台中",
                    "tainan" => "台南",
                    "kaohsiung" => "高雄",
                    "yunlin" => "雲林",
                    "pingtung" => "屏東",
                    "hsinchu" => "新竹",
                    "miaoli" => "苗栗",
                    "changhua" => "彰化",
                    "nantou" => "南投",
                    "chiayi" => "嘉義",
                    "yilan" => "宜蘭",
                    "hualien" => "花蓮",
                    "taitung" => "台東",
                    "keelung" => "基隆",
                    _ => request.City
                };
            }

            var combinedList = new List<object>();

            // 步驟 B：【房屋搜尋分支】當分類為 "all" 或 "room" 時執行
            if (request.Category == "all" || request.Category == "room")
            {
                var houseQuery = from house in _context.Rent_Houses
                                 join rule in _context.HouseRules on house.Id equals rule.HouseId into rules
                                 from rule in rules.DefaultIfEmpty()
                                 join loc in _context.Location_Districts on house.DistrictId equals loc.DistrictId into locations
                                 from loc in locations.DefaultIfEmpty()
                                 where house.Status == 1
                                 select new { house, rule, loc };

                // 1. 房屋價格篩選
                if (request.PriceMin > 0)
                    houseQuery = houseQuery.Where(x => x.house.RentPrice >= (int)request.PriceMin);
                if (request.PriceMax < 50000)
                    houseQuery = houseQuery.Where(x => x.house.RentPrice <= (int)request.PriceMax);

                // 2. 房屋縣市篩選 (比對聯結表的 CityName 或是房屋自身的 Address 欄位)
                if (!string.IsNullOrEmpty(cityNameKeyword))
                {
                    houseQuery = houseQuery.Where(x => (x.loc != null && x.loc.CityName.Contains(cityNameKeyword)) || x.house.Address.Contains(cityNameKeyword));
                }

                // 3. 生活習慣布林篩選
                if (request.Smoking.HasValue)
                    houseQuery = houseQuery.Where(x => x.rule != null && x.rule.Smoke == request.Smoking.Value);

                if (request.Pets.HasValue)
                    houseQuery = houseQuery.Where(x => x.rule != null && x.rule.Pet == request.Pets.Value);

                // 4. 作息時間篩選
                if (request.Routines.Contains("早睡早起"))
                    houseQuery = houseQuery.Where(x => x.rule != null && x.rule.SleepTime <= new TimeOnly(23, 0));

                if (request.Routines.Contains("夜貓子"))
                    houseQuery = houseQuery.Where(x => x.rule != null && x.rule.SleepTime >= new TimeOnly(23, 0));

                var houseResults = await houseQuery.Select(x => new
                {
                    Id = x.house.Id,
                    Name = x.house.Name,
                    Address = x.house.Address,
                    RentPrice = x.house.RentPrice,
                    Description = x.house.Description,
                    Status = x.house.Status,

                    CityName = x.loc != null ? x.loc.CityName : "未知縣市",
                    DistrictName = x.loc != null ? x.loc.DistrictName : "未知區域",

                    HouseId = x.house.Id,
                    SleepTime = x.rule != null ? x.rule.SleepTime : null,
                    WakeTime = x.rule != null ? x.rule.WakeTime : null,
                    CleanLevel = x.rule != null ? (int?)x.rule.CleanLevel : null,
                    NoiseTolerance = x.rule != null ? (int?)x.rule.NoiseTolerance : null,
                    Pet = x.rule != null ? x.rule.Pet : null,
                    Smoke = x.rule != null ? x.rule.Smoke : null,
                    AdvancedRules = x.rule != null ? x.rule.AdvancedRules : null
                }).ToListAsync();

                combinedList.AddRange(houseResults);
            }

            // 步驟 C：【工具技能搜尋分支】當分類為 "all" 或 "product" 時執行
            if (request.Category == "all" || request.Category == "product")
            {
                var productQuery = _context.Rent_Products.AsQueryable();

                // 1. 工具價格篩選
                if (request.PriceMin > 0)
                    productQuery = productQuery.Where(x => x.Price >= request.PriceMin);
                if (request.PriceMax < 50000)
                    productQuery = productQuery.Where(x => x.Price <= request.PriceMax);

                // 2. 工具縣市篩選 (利用工具內建的 Address 欄位做模糊字串檢索)
                if (!string.IsNullOrEmpty(cityNameKeyword))
                {
                    productQuery = productQuery.Where(x => x.Address != null && x.Address.Contains(cityNameKeyword));
                }

                var productResults = await productQuery.Select(x => new
                {
                    Id = x.Id,
                    AccountId = x.AccountId,
                    Name = x.Name,
                    Category = x.Category,
                    Description = x.Description,
                    Price = x.Price,
                    PriceUnit = x.PriceUnit,
                    Deposit = x.Deposit,
                    IsOnline = x.IsOnline,
                    Quantity = x.Quantity,
                    OwnTool = x.OwnTool,
                    RequiredKnowledge = x.RequiredKnowledge,
                    Address = x.Address,
                    UsageRequirements = x.UsageRequirements,
                    UsageTerms = x.UsageTerms,

                    // 補上空值欄位，避免前端格式錯亂
                    CityName = "",
                    DistrictName = ""
                }).ToListAsync();

                combinedList.AddRange(productResults);
            }

            // 步驟 D：依前端指定的 SortOrder 進行最終排序
            if (request.SortOrder == "oldest")
            {
                return combinedList.OrderBy(x => GetIdFromAnonymous(x)).ToList();
            }
            else
            {
                return combinedList.OrderByDescending(x => GetIdFromAnonymous(x)).ToList();
            }
        }

        // 💡 排序用輔助方法：安全地從動態匿名物件中抽取 Id 進行排序
        private int GetIdFromAnonymous(object obj)
        {
            var prop = obj.GetType().GetProperty("Id");
            if (prop != null)
            {
                return (int)(prop.GetValue(obj) ?? 0);
            }
            return 0;
        }
    }
}