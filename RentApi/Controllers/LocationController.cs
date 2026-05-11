using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RentApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase {
        private readonly LocationService _service;

        public LocationController(LocationService service) {
            _service = service;
        }

        // GET: api/location/cities
        [HttpGet("cities")]
        public async Task<IActionResult> GetCities() {
            var data = await _service.GetCitiesAsync();
            return Ok(data);
        }

        // GET: api/location/districts?cityId=1
        [HttpGet("districts")]
        public async Task<IActionResult> GetDistricts([FromQuery] int cityId) {
            var data = await _service.GetDistrictsByCityIdAsync(cityId);
            return Ok(data);
        }

        // GET: api/location/zipcode?districtId=10
        [HttpGet("zipcode")]
        public async Task<IActionResult> GetZipCode([FromQuery] int districtId) {
            var zip = await _service.GetZipCodeAsync(districtId);
            return Ok(zip);
        }
        [HttpGet("user-location/{userId}")]
        public async Task<IActionResult> GetUserLocation(int userId) {
            var data = await _service.GetUserLocationAsync(userId);

            if (data == null)
                return NotFound();

            return Ok(data);
        }
    }
}
