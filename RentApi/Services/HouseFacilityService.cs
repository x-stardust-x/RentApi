using RentApi.Data;
using RentApi.Models;
using RentApi.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace RentApi.Services
{
    public class HouseFacilityService
    {
        private readonly AppDbContext _context;

        public HouseFacilityService(AppDbContext context) => _context = context;

        // 1. 撈出全站所有設施
        public async Task<List<FacilityReadDto>> GetAllFacilitiesAsync()
        {
            return await _context.SystemFacilities
                .Select(f => new FacilityReadDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Category = f.Category,
                    IconClass = f.IconClass
                }).ToListAsync();
        }

        // ===================================================================
        // 2. 撈出特定房屋持有的設施清單【完美校對版】
        // ===================================================================
        public async Task<List<FacilityReadDto>> GetHouseFacilitiesAsync(int houseId)
        {
            // 💡 步驟一：先去中間表，找出哪幾筆資料的 HouseId 符合傳進來的 houseId
            // 根據妳的資料庫圖表，這裡精確對齊小寫 d 的 hf.HouseId
            var selectedFacilityIds = await _context.HouseFacilities
                .Where(hf => hf.HouseId == houseId)
                .Select(hf => hf.FacilityId)
                .ToListAsync();

            // 💡 步驟二：去系統設施主表把這些符合 ID 的名稱跟圖示撈出來
            return await _context.SystemFacilities
                .Where(f => selectedFacilityIds.Contains(f.Id))
                .Select(f => new FacilityReadDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Category = f.Category,
                    IconClass = f.IconClass
                }).ToListAsync();
        }

        // 3. 更新特定房屋的設施項目
        public async Task UpdateHouseFacilitiesAsync(HouseFacilityUpdateDto dto)
        {
            // 💡 這裡同步修正為小寫 d 的 hf.HouseId
            var oldFacilities = await _context.HouseFacilities
                .Where(hf => hf.HouseId == dto.HouseId).ToListAsync();
            _context.HouseFacilities.RemoveRange(oldFacilities);

            if (dto.SelectedFacilityIds == null || !dto.SelectedFacilityIds.Any())
            {
                await _context.SaveChangesAsync();
                return;
            }

            foreach (var facilityId in dto.SelectedFacilityIds)
            {
                if (facilityId <= 0) continue;

                var facilityExists = await _context.SystemFacilities.AnyAsync(f => f.Id == facilityId);
                if (!facilityExists)
                {
                    throw new Exception($"錯誤：系統中找不到設施 ID 為 {facilityId} 的資料！");
                }

                _context.HouseFacilities.Add(new HouseFacility
                {
                    HouseId = dto.HouseId, // 💡 如果妳的 Model 裡宣告的是 HouseId 數字欄位，請對齊妳的 Model 宣告
                    FacilityId = facilityId
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}