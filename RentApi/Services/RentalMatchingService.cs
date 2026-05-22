using Microsoft.EntityFrameworkCore;
using RentApi.Data;
using RentApi.Interfaces;
using RentApi.Models;
using RentApi.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

                            // 🌟 核心升級：直接導入全新整合表，利用 house.DistrictId 抓出縣市區名！
                        join loc in _context.Location_Districts on house.DistrictId equals loc.DistrictId into locations
                        from loc in locations.DefaultIfEmpty()

                        where house.Status == 1 // 只要上架的房屋
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

                            // 🌟 補足地點大金剛欄位，餵飽前端的篩選器！
                            CityName = loc != null ? loc.CityName : "未知縣市",
                            DistrictName = loc != null ? loc.DistrictName : "未知區域",
                            ZipCode = loc != null ? loc.ZipCode.ToString() : "",

                            // 生活習慣規範
                            HouseId = rule != null ? rule.HouseId : null,
                            SleepTime = rule != null ? (TimeOnly?)rule.SleepTime : null,
                            WakeTime = rule != null ? (TimeOnly?)rule.WakeTime : null,
                            CleanLevel = rule != null ? (int?)rule.CleanLevel : null,
                            NoiseTolerance = rule != null ? (int?)rule.NoiseTolerance : null,
                            Pet = rule != null ? rule.Pet : null,
                            Smoke = rule != null ? rule.Smoke : null,

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

                            // 🌟 核心升級：詳情頁面也同步加入新整合表 JOIN
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

                            // 🌟 補齊地點欄位
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

                            // 🌟 核心升級：假設你的工具表也有 DistrictId 或者是 Address 反查，這裡先用 DistrictId 進行關聯
                        

                        select new Match_ProductDto
                        {
                            Id = product.Id,
                            AccountId = product.AccountId,
                            DistrictId = null, // 🌟 記得補上
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

                            CityName = "",
                            DistrictName = "",

                            UserName = account != null ? account.Username : "未知提供者",
                            Bio = user != null ? user.Bio : "這位提供者還沒寫自我介紹",
                            Rating = user != null ? user.Rating : 0,
                            ReviewCount = user != null ? user.ReviewCount : 0,
                            RealName = user != null ? user.RealName : "未知提供者"
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
    }
}