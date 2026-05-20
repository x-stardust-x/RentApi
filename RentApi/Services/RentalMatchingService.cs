using Microsoft.EntityFrameworkCore;
using RentApi.Data;
using RentApi.Interfaces;
using RentApi.Models;
using RentApi.Models.DTO;


namespace RentApi.Services
{
    public class RentalMatchingService : IRentalMatchingService
    {
        private readonly AppDbContext _context;

        public RentalMatchingService(AppDbContext context)
        {
            _context = context;
        }

        // 抓取所有【上架中】的房子 ( 回傳清單 )
        //public async Task<IEnumerable<Match_HouseDto>> GetRentalAsync(int id)
        public async Task<IEnumerable<Match_HouseDto>> GetRentalAsync()
        {
            // 使用 LINQ Join 將 RentHouse 和 HouseRules 兩張資料表做關聯查詢
            var query = from house in _context.Rent_Houses

                        join rule in _context.HouseRules on house.Id equals rule.HouseId into rules
                        from rule in rules.DefaultIfEmpty()
                        
                        join account in _context.Account on house.AccountId equals account.Id into accounts
                        from account in accounts.DefaultIfEmpty() // 使用Left join，預防帳號被刪除時房子抓不到

                        join user in _context.User on (account != null ? account.Id : -1) equals user.AccountId into users
                        from user in users.DefaultIfEmpty()

                        where house.Status == 1 // 只要上架的房屋
                        select new Match_HouseDto
                        {
                            // 房屋基本資料
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

                            // 生活習慣規範
                            HouseId = rule != null ? rule.HouseId : null,
                            SleepTime = rule != null ? (TimeOnly?)rule.SleepTime : null,
                            WakeTime = rule != null ? (TimeOnly?)rule.WakeTime : null,
                            CleanLevel = rule != null ? (int?)rule.CleanLevel : null,
                            NoiseTolerance = rule != null ? (int?)rule.NoiseTolerance : null,
                            Pet = rule != null ? rule.Pet : null,
                            Smoke = rule != null ? rule.Smoke : null,

                            // 真實名字
                            RealName = user != null ? user.RealName : "未知提供者",

                        };

            return await query.ToListAsync();
        }


        // 根據 ID 抓取單一房屋【詳情】( 回傳單筆 )
        public async Task<Match_HouseDto?> GetRentalByAsync(int id)
        {

            var ruleCount = await _context.HouseRules.CountAsync(r => r.HouseId == id);
            Console.WriteLine($"除錯：資料庫中 HouseId 為 {id} 的規則筆數有 {ruleCount} 筆");

            var query = from house in _context.Rent_Houses

                            // 原有的 HouseRules
                        join rule in _context.HouseRules on house.Id equals rule.HouseId into rules
                        from rule in rules.DefaultIfEmpty()

                            // 關聯 Account 表取 UserName
                        join account in _context.Account on house.AccountId equals account.Id into accounts
                        from account in accounts.DefaultIfEmpty()

                            // 關聯 User 表取 Bio、Rating、ReviewCount
                        join user in _context.User on (account != null ? account.Id : -1) equals user.AccountId into users
                        from user in users.DefaultIfEmpty()

                        where house.Id == id
                        select new Match_HouseDto
                        {
                            // 房屋基本資料
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

                            // 出租人資料
                            // Account 表 UserName
                            UserName = account != null ? account.Username : "未知房東",

                            // User 表 Bio
                            Bio = user != null ? user.Bio : "這位出租人還沒寫自我介紹",

                            Rating = user != null ? user.Rating : 0,
                            ReviewCount = user != null ? user.ReviewCount : 0,

                            // 生活習慣規範
                            HouseId = rule != null ? rule.HouseId : null,
                            SleepTime = rule != null ? rule.SleepTime : null,
                            WakeTime = rule != null ? rule.WakeTime : null,
                            CleanLevel = rule != null ? (int?)rule.CleanLevel : null,
                            NoiseTolerance = rule != null ? (int?)rule.NoiseTolerance : null,
                            Pet = rule != null ? rule.Pet : null,
                            Smoke = rule != null ? rule.Smoke : null,


                            // 租賃物圖片，先給空陣列，避免前端報錯
                            ImageUrls = new List<string>(),


                            // 真實名字
                            RealName = user !=  null ? user.RealName : "未知提供者",

                            //// 租賃物圖片
                            //ImageUrls = _context.House_Images
                            //                    .Where(img => img.HouseId == house.Id)
                            //                    .Select(img => img.Url)
                        };


                        // 真正執行 SQL 撈出房屋本體
                        var houseResult = await query.FirstOrDefaultAsync();

                        // 第二階段：如果成功抓到房屋，才去獨立撈取該房屋的所有圖片，並塞回去
                        if (houseResult != null)
                        {
                            var images = await _context.House_Images
                                                       .Where(img => img.HouseId == id)
                                                       .Select(img => img.Url)
                                                       .ToListAsync(); // 在這裡安全地用非同步 List 撈出

                            houseResult.ImageUrls = images; // 把圖片陣列塞回物件中
                        }

                        return houseResult;


            //return await query.FirstOrDefaultAsync();
        }



        // 實作一：抓取所有工具技能
        public async Task<IEnumerable<Match_ProductDto>> GetProductAsync()
        {
            //return await _context.Rent_Products // 注意：如果你的 DbSet 名字不同（例如叫 Products），請改成你的名字


            var query = from product in _context.Rent_Products


                        //// 原有的 HouseRules
                        //join rule in _context.HouseRules on house.Id equals rule.HouseId into rules
                        //from rule in rules.DefaultIfEmpty()


                            // 關聯 Account 表，取得提供者帳號名稱
                        join account in _context.Account
                        on product.AccountId equals account.Id into accounts
                        from account in accounts.DefaultIfEmpty()

                            // 關聯 User 表，取得提供者自我介紹、評分、評價數
                        join user in _context.User
                            on (account != null ? account.Id : -1) equals user.AccountId into users
                        from user in users.DefaultIfEmpty()


                        select new Match_ProductDto
                        {
                            Id = product.Id,
                            AccountId = product.AccountId,
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

                            // 提供者資料
                            UserName = account != null ? account.Username : "未知提供者",
                            Bio = user != null ? user.Bio : "這位提供者還沒寫自我介紹",
                            Rating = user != null ? user.Rating : 0,
                            ReviewCount = user != null ? user.ReviewCount : 0,

                            // 真實名字
                            RealName = user != null ? user.RealName : "未知提供者",
                        };
            return await query.ToListAsync();
        }

        // 實作二：依據 ID 抓取單一工具技能詳情
        public async Task<Match_ProductDto?> GetProductByIdAsync(int id)
        {
            var p = await _context.Rent_Products.FirstOrDefaultAsync(x => x.Id == id); // 💡 這裡也一樣要對齊你的 DbSet 名字
            if (p == null) return null;


            var query = from product in _context.Rent_Products

                            // 關聯 Account 表，取得提供者帳號名稱
                        join account in _context.Account
                        on product.AccountId equals account.Id into accounts
                        from account in accounts.DefaultIfEmpty()

                            // 關聯 User 表，取得提供者自我介紹、評分、評價數
                        join user in _context.User
                            on (account != null ? account.Id : -1) equals user.AccountId into users
                        from user in users.DefaultIfEmpty()

                        where product.Id == id


                        select new Match_ProductDto
                        {
                            Id = p.Id,
                            AccountId = p.AccountId,
                            Name = p.Name,
                            Category = p.Category,
                            Description = p.Description,
                            Price = p.Price,
                            PriceUnit = p.PriceUnit,
                            Deposit = p.Deposit,
                            IsOnline = p.IsOnline,
                            Quantity = p.Quantity,
                            OwnTool = p.OwnTool,
                            RequiredKnowledge = p.RequiredKnowledge,
                            Address = p.Address,

                            // 提供者資料
                            UserName = account != null ? account.Username : "未知提供者",
                            Bio = user != null ? user.Bio : "這位提供者還沒寫自我介紹",
                            Rating = user != null ? user.Rating : 0,
                            ReviewCount = user != null ? user.ReviewCount : 0,

                            // 真實名字
                            RealName = user !=  null ? user.RealName : "未知提供者",
                        };
            return await query.FirstOrDefaultAsync();
        }

    }
}
