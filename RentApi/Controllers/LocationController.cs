using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RentApi.Services;
namespace RentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly LocationService _service;

        public LocationController(LocationService service)
        {
            _service = service;
        }

        // GET: api/location/cities
        [HttpGet("cities")]
        public async Task<IActionResult> GetCities()
        {
            var data = await _service.GetCitiesAsync();
            return Ok(data);
        }

        // 🌟 GET: api/location/districts-all (前端會呼叫這支，一次拿完全部)
        [HttpGet("districts-all")]
        public async Task<IActionResult> GetAllDistricts()
        {
            var data = await _service.GetDistrictsAsync();
            return Ok(data);
        }

        // 🌟 GET: api/location/districts?cityName=高雄市
        // 【修改重點】這裡把 int cityId 改成了 string cityName
        [HttpGet("districts")]
        public async Task<IActionResult> GetDistricts([FromQuery] string cityName)
        {
            var data = await _service.GetDistrictsByCityNameAsync(cityName);
            return Ok(data);
        }

        // GET: api/location/zipcode?districtId=10
        [HttpGet("zipcode")]
        public async Task<IActionResult> GetZipCode([FromQuery] int districtId)
        {
            var zip = await _service.GetZipCodeAsync(districtId);
            return Ok(zip);
        }

        // GET: api/location/user-location/5
        [HttpGet("user-location/{userId}")]
        public async Task<IActionResult> GetUserLocation(int userId)
        {
            var data = await _service.GetUserLocationAsync(userId);
            if (data == null) return NotFound();
            return Ok(data);
        }
    }
}

