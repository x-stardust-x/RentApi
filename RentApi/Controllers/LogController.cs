using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RentApi.Models;
using RentApi.Services;
using RentApi.Models.DTO;

namespace RentApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase {
        private readonly LogService _service;
        public LogController(LogService service) {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> Get() {
            var res = await _service.GetAllAsync();
            if (res.Count == 0) {
                return NotFound("No Data");
            }
            return Ok(res);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByUserId(int id) {
            var res = await _service.GetByUserIdAsync(id);
            if (res.Count == 0) {
                return NotFound("No Data");
            }
            return Ok(res);
        }
        [HttpPost]
        public async Task<IActionResult> Post(System_LogDto log) {
            var res = await _service.PostAsync(log);
            if (res == null) {
                return BadRequest("Failed to create log");
            }
            return Ok(res);
        }
    }
}
