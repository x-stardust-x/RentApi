using Microsoft.AspNetCore.Mvc;
using RentApi.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RentApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase {
        AppDbContext _context;
        public AdminController(AppDbContext context) { 
            _context = context;
        }
        // GET: api/<AdminController>
        [HttpGet]
        public IActionResult Get() {
            var res = _context.Admin;
            return Ok(res);
        }

        // GET api/<AdminController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id) {
            return Ok();
        }

        // POST api/<AdminController>
        [HttpPost]
        public void Post([FromBody] string value) {
        }

        // PUT api/<AdminController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value) {
        }

        // DELETE api/<AdminController>/5
        [HttpDelete("{id}")]
        public void Delete(int id) {
        }
    }
}
