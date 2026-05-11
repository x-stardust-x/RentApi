using Microsoft.EntityFrameworkCore;
using RentApi.Data;
using RentApi.Models;
using RentApi.Models.DTO;

public class LocationService {
    private readonly AppDbContext _db;

    public LocationService(AppDbContext db) {
        _db = db;
    }

    // 取得所有城市
    public async Task<List<City>> GetCitiesAsync() {
        return await _db.City
            .OrderBy(x => x.Id)
            .ToListAsync();
    }

    // 依 CityId 取得區域
    public async Task<List<District>> GetDistrictsByCityIdAsync(int cityId) {
        return await _db.District
            .Where(x => x.CityId == cityId)
            .OrderBy(x => x.Id)
            .ToListAsync();
    }

    // 取得郵遞區號
    public async Task<int?> GetZipCodeAsync(int districtId) {
        var district = await _db.District
            .FirstOrDefaultAsync(x => x.Id == districtId);

        return district?.ZipCode;
    }

    public async Task<object?> GetUserLocationAsync(int userId) {
        var result = await (
             from u in _db.User
             join d in _db.District on u.DistrictId equals d.Id
             join c in _db.City on d.CityId equals c.Id
             where u.Id == userId
             select new {
                 u.Id,
                 c.CityName,
                 d.DistrictName,
                 d.ZipCode
             }
         ).FirstOrDefaultAsync();
        return result;
    }
}