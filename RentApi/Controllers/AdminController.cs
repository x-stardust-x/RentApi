using Microsoft.AspNetCore.Mvc;
using RentApi.Data;
using RentApi.Models;

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
            var res = _context.Admin.FirstOrDefault(a => a.Id == id);
            if (res == null)
                return NotFound();
            return Ok(res);
        }

        // POST api/<AdminController>
        [HttpPost]
        public IActionResult Post([FromBody] Admin admin) {
            if (admin == null)
                return BadRequest();
            _context.Admin.Add(admin);
            _context.SaveChanges();
            return Ok(admin);
        }

        // PUT api/<AdminController>/5
        // 更新管理員資料
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Admin admin) {
            if (admin == null || admin.Id != id)
                return BadRequest();
            var existingAdmin = _context.Admin.FirstOrDefault(a => a.Id == id);
            if (existingAdmin == null)
                return NotFound();
            existingAdmin.Username = admin.Username;
            existingAdmin.Pwd = admin.Pwd;
            existingAdmin.Email = admin.Email;
            existingAdmin.Phone = admin.Phone;
            _context.SaveChanges();
            return Ok(existingAdmin);
        }
        //password reset
        [HttpPut("reset-password/{id}")]
        public IActionResult ResetPassword(int id, [FromBody] Admin admin) {
            if (admin == null || admin.Id != id)
                return BadRequest();
            var existingAdmin = _context.Admin.FirstOrDefault(a => a.Id == id);
            if (existingAdmin == null)
                return NotFound();
            existingAdmin.Pwd = "0000";
            _context.SaveChanges();
            return Ok(existingAdmin);
        }

        // DELETE api/<AdminController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) {
            var existingAdmin = _context.Admin.FirstOrDefault(a => a.Id == id);
            if (existingAdmin == null)
                return NotFound();
            _context.Admin.Remove(existingAdmin);
            _context.SaveChanges();
            return Ok(existingAdmin);
        }
    }
}
