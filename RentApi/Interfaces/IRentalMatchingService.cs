//using Match_HouseDetailDto.DTOs;

using RentApi.Models.DTO;
using YourProjectNamespace.Dtos;

namespace RentApi.Interfaces
{
    public interface IRentalMatchingService
    {
        // 取得所有上架中的房屋清單
        Task<IEnumerable<Match_HouseDto>> GetRentalAsync();
        // 根據 ID 取得單一詳情
        Task<Match_HouseDto?> GetRentalByAsync(int id);
        Task<IEnumerable<Match_ProductDto>> GetProductAsync();
        Task<Match_ProductDto?> GetProductByIdAsync(int id);
        Task<IEnumerable<object>> SearchRentalsAsync(RentalFilterRequestDto request);
    }
}
