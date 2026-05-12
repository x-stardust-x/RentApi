using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RentApi.Data;
using RentApi.Models;
using RentApi.Models.DTO;
using System.Diagnostics;
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
            var res =  _db.Admin.FirstOrDefault(a => a.Email == dto.Email);
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
                    new Claim(ClaimTypes.Name, res.Username),
                    new Claim(ClaimTypes.Role, "admin"),
                    new Claim("AdminId", res.Id.ToString())
                };
            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddSeconds(360),
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new {
                message = "登入成功",
                token = jwt,
                username = res.Username,
                role = "admin"
            });
        }
        [HttpPost("login/member")]
        public IActionResult Memberlogin(LoginDto dto) {
            var res = _db.Account.FirstOrDefault(a => a.Email == dto.Email);
            if (res == null) {
                return Unauthorized(new {
                    message = "帳號不存在"
                });
            }
            var user = _db.User.FirstOrDefault(a => a.AccountId == res.Id);
            Console.WriteLine(user.Id);
            if (!PasswordHelper.Verify(dto.Pwd, res.Pwd)) {
                return Unauthorized(new {
                    message = "密碼錯誤"
                });
            }

            var iden = "";

            if(res.Identity == Identity.young) {
                iden = "young";
            }
            else {
                iden = "old";
            }

            res.LastLoginAt = DateTime.Now;
            _db.SaveChanges();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
                {
                    new Claim(ClaimTypes.Name, res.Username),
                    new Claim(ClaimTypes.Role, iden),
                    new Claim("UserId", user.Id.ToString())
                };
            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddSeconds(360),
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new {
                message = "登入成功",
                token = jwt,
                username = res.Username,
                role = iden
            });
        }
        [HttpPost("register")]
        public IActionResult Register(RegisterDto dto) {
            // 🔥 1. 檢查帳號是否存在
            var exist = _db.Account.Any(x => x.Email == dto.Account.Email);
            if (exist)
                return BadRequest(new { message = "帳號已存在" });

            string hashedPwd = PasswordHelper.Hash(dto.Account.Pwd);

            // 🔥 2. 建立 Account
            var account = new Account {
                Username = dto.Account.Username,
                Pwd = hashedPwd, // 存哈希後的密碼
                Email = dto.Account.Email,
                Birthday = dto.Account.Birthday,
                Age = dto.Account.Age,
                Identity = (Identity)dto.Account.Identity,
            };

            _db.Account.Add(account);
            _db.SaveChanges(); // 先存才能拿 Id

            // 🔥 3. 建立 User
            var user = new User {
                AccountId = account.Id,
                //Nickname = dto.User.Nickname,
                //Email = dto.User.Email,
                //Address = dto.User.Address,
                CreateAt = DateTime.Now
            };

            _db.User.Add(user);
            _db.SaveChanges();

            var user_habit = new User_Habit {
                UserId = user.Id,
            };

            _db.User_Habit.Add(user_habit);
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
