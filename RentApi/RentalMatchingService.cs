using Microsoft.EntityFrameworkCore;
using RentApi.Data;
using RentApi.Interfaces;
using RentApi.Models.DTO;
//using Match_HouseDetailDto;


namespace RentApi
{
    public class RentalMatchingService : IRentalMatchingService
    {
        private readonly AppDbContext _context;

        public RentalMatchingService(AppDbContext context)
        {
            _context = context;
        }

        // 抓取所有【上架中】的房子
        public async Task<IEnumerable<Match_HouseDto>> GetRentalAsync()
        {
            return await _context.Rent_House
                .Where(h => h.Status == 1) // 篩選上架中
                .Select(h => new Match_HouseDto {
                    Id = h.Id,
                    Name = h.Name,



                })
                .ToListAsync();
        }

        // 在 RentalMatchingService 類別內新增
        public async Task<Match_HouseDto?> GetRentalByAsync(int id)
        {
            var house = await _context.Rent_House.FindAsync(id);

            if (house == null) return null;

            return new Match_HouseDto
            {
                Id = house.Id,
                Name = house.Name,
                Description = house.Description,
                RentPrice = house.RentPrice,

                // 如有其他欄位請一併對應
            };
        }





        //// 2. 根據 ID 抓取詳情
        //public async Task<Match_HouseDto?> GetRentalByIdAsync(int id)
        //{
        //    var house = await _context.Rent_House.FindAsync(id);

        //    if (house == null) return null;

        //    return new Match_HouseDto
        //    {
        //        Id = house.Id,
        //        Name = house.Name,
        //        Description = house.Description,
        //        RentPrice = house.RentPrice,
        //        AreaSize = house.AreaSize,
        //        IncludeWifi = house.IncludeWifi,
        //        // ... 補齊其他詳情欄位
        //    };
        //}


    }
}
