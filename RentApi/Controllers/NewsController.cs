using Microsoft.AspNetCore.Mvc;
using RentApi.Models;
using RentApi.Models.DTO;
using RentApi.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RentApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase {
        private readonly NewsService _service;
        public NewsController(NewsService service) {
            _service = service;
        }
        // GET: api/<NewsController>
        [HttpGet]
        public async Task<IActionResult> Get() {
            var result = await _service.GetAllAsync();
            if (result == null) {
                return NotFound("No announcements found.");
            }
            return Ok(result);
        }

        // GET api/<NewsController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id) {
            var result = await _service.GetByIdAsync(id);
            if(result == null) {
                return NotFound("Announcement not found.");
            }
            return Ok(result);
        }

        // POST api/<NewsController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] System_AnnouncementDto value) {
            var result = await _service.PostAsync(value);
            return Ok(result);
        }

        // PUT api/<NewsController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] System_AnnouncementDto value) {
           var result = await _service.PutAsync(id, value);
            if(result == null) {
                return NotFound("Announcement not found.");
            }
            return Ok(result);
        }

        // DELETE api/<NewsController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) {
            var result = await _service.DeleteAsync(id);
            if (result == null) {
                return NotFound("Announcement not found.");
            }
            return Ok(result);
        }
    }
}
