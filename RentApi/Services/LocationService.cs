using Microsoft.EntityFrameworkCore;
using RentApi.Data;
using RentApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentApi.Services // 💡 請確保 namespace 路由與你專案的資料夾路徑對齊
{
    public class LocationService
    {
        private readonly AppDbContext _db;

        public LocationService(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// 1. 取得所有城市（從 Location_District 表中撈出「不重複」的縣市名稱清單）
        /// </summary>
        public async Task<List<string>> GetCitiesAsync()
        {
            return await _db.Location_Districts
                .Select(x => x.CityName)
                .Distinct()
                .OrderBy(x => x) // 順便幫你按筆劃/字母排序，選單看起來更整齊
                .ToListAsync();
        }

        /// <summary>
        /// 2. 取得所有區域（前端在 ngOnInit 初始化時，會呼叫這個一次拿完全台灣行政區）
        /// </summary>
        public async Task<List<Location_District>> GetDistrictsAsync()
        {
            return await _db.Location_Districts
                .OrderBy(x => x.DistrictId)
                .ToListAsync();
        }

        /// <summary>
        /// 3. 依據縣市名稱（字串）取得對應的行政區清單
        /// </summary>
        public async Task<List<Location_District>> GetDistrictsByCityNameAsync(string cityName)
        {
            return await _db.Location_Districts
                .Where(x => x.CityName == cityName)
                .OrderBy(x => x.DistrictId)
                .ToListAsync();
        }

        /// <summary>
        /// 4. 依行政區主鍵 ID 取得郵遞區號
        /// </summary>
        public async Task<int?> GetZipCodeAsync(int districtId)
        {
            var district = await _db.Location_Districts
                .FirstOrDefaultAsync(x => x.DistrictId == districtId);

            return district?.ZipCode;
        }

        /// <summary>
        /// 5. 依使用者 ID 取得其設定的居住地資訊（省去多表 JOIN，效能大提升！）
        /// </summary>
        public async Task<object?> GetUserLocationAsync(int userId)
        {
            var result = await (
                 from u in _db.User
                 // 🌟 完美對接：透過使用者的 DistrictId 關聯到 Location_District 表
                 join d in _db.Location_Districts on u.DistrictId equals d.DistrictId
                 where u.Id == userId
                 select new
                 {
                     u.Id,
                     d.CityName,
                     d.DistrictName,
                     d.ZipCode
                 }
             ).FirstOrDefaultAsync();

            return result;
        }
    }
}