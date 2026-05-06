using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RentApi.Data;
using RentApi.Models;
using RentApi.Models.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RentApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext db, IConfiguration config) {
            _db = db;
            _config = config;
        }
        public record ApiResponse(string Message, bool IsSuccessful);

        [HttpGet("getallmember")]
        public IActionResult AllMember() {
            var res = _db.Account;
            return Ok(res);
        }
        [Authorize(Roles ="admin")]
        [HttpGet("getalladmin")]
        public IActionResult AllAdmin() {
            var res = _db.Admin;
            return Ok(res);
        }

        [HttpPost("login/admin")]
        public IActionResult AdminLogin(LoginDto dto) {
            var res =  _db.Admin.FirstOrDefault(a => a.Name == dto.Username);
            if (res == null) {
                return Unauthorized(new {
                    message = "帳號不存在"
                });
            }
            if (res.Pwd != dto.Pwd) {
                return Unauthorized(new {
                    message = "密碼錯誤"
                });
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
                {
                    new Claim(ClaimTypes.Name, res.Name),
                    new Claim(ClaimTypes.Role, "admin"),
                    new Claim("AccountId", res.Id.ToString())
                };
            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddSeconds(60),
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new {
                message = "登入成功",
                token = jwt,
                username = res.Name,
                role = "admin"
            });
        }
        [HttpPost("login/member")]
        public IActionResult Memberlogin(LoginDto dto) {
            var res = _db.Account.FirstOrDefault(a => a.Username == dto.Username);
            if (res == null) {
                return Unauthorized(new {
                    message = "帳號不存在"
                });
            }
            if (!PasswordHelper.Verify(dto.Pwd, res.Pwd)) {
                return Unauthorized(new {
                    message = "密碼錯誤"
                });
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
                {
                    new Claim(ClaimTypes.Name, res.Username),
                    new Claim(ClaimTypes.Role, "member"),
                    new Claim("AccountId", res.Id.ToString())
                };
            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddSeconds(60),
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new {
                message = "登入成功",
                token = jwt,
                username = res.Username,
                role = "member"
            });
        }
        [HttpPost("register")]
        public IActionResult Register(RegisterDto dto) {
            // 🔥 1. 檢查帳號是否存在
            var exist = _db.Account.Any(x => x.Username == dto.Account.Username);
            if (exist)
                return BadRequest(new { message = "帳號已存在" });

            string hashedPwd = PasswordHelper.Hash(dto.Account.Pwd);

            // 🔥 2. 建立 Account
            var account = new Account {
                Username = dto.Account.Username,
                //Pwd = BCrypt.Net.BCrypt.HashPassword(dto.Account.Password),
                Pwd = hashedPwd, // 存哈希後的密碼
                Identity = (Identity)dto.Account.Identity,
                IsDelete = false
            };

            _db.Account.Add(account);
            _db.SaveChanges(); // 先存才能拿 Id

            // 🔥 3. 建立 User
            var user = new User {
                AccountId = account.Id,
                Nickname = dto.User.Nickname,
                Email = dto.User.Email,
                Address = dto.User.Address
            };

            _db.User.Add(user);
            _db.SaveChanges();

            return Ok(new {
                message = "註冊成功"
            });
        }
        [Authorize]
        [HttpGet("test")]
        public IActionResult Test() {
            return Ok("你有登入");
        }
    }
}
