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
            string iden = "admin";
            if(res.isSuper == true) {
                iden = "super";
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
                {
                    new Claim(ClaimTypes.Name, res.Username),
                    new Claim(ClaimTypes.Role, iden),
                    new Claim("AdminId", res.Id.ToString())
                };
            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
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

            var iden = "user";

            //if(res.Identity == Identity.young) {
            //    iden = "young";
            //}
            //else {
            //    iden = "old";
            //}

            res.LastLoginAt = DateTime.Now;
            _db.SaveChanges();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
                {
                    new Claim(ClaimTypes.Name, res.Username),
                    new Claim(ClaimTypes.Role, iden),
                    new Claim("AccountId", user.AccountId.ToString()),
                    new Claim("UserId", user.Id.ToString())
                };
            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
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
        public async Task<IActionResult> Register(RegisterDto dto) {
            // 🔥 1. 檢查帳號是否存在
            var exist = _db.Account.Any(x => x.Email == dto.Account.Email);

            if (exist)
                return BadRequest(new { message = "帳號已存在" });
            if (dto.Account.Age < 18) {
                return BadRequest(new { message = "年齡必須大於18歲" });
            }

            int isYoung = dto.Account.Age <= 65 ? 0 : 1; // 假設 65 歲以下代表年輕人，否則代表年長者
            string hashedPwd = PasswordHelper.Hash(dto.Account.Pwd);

            using var transaction = _db.Database.BeginTransaction();

            try {
                var account = new Account {
                    Username = dto.Account.Username,
                    Pwd = hashedPwd, // 存哈希後的密碼
                    Email = dto.Account.Email,
                    Birthday = dto.Account.Birthday,
                    Age = dto.Account.Age,
                    Identity = (Identity)isYoung,
                    Status = true
                };
                _db.Account.Add(account);

                // 🔥 2. 建立 Account
                var user = new User {
                    Account = account,
                    DistrictId = 1, //防止error
                    //Nickname = dto.User.Nickname,
                    //Email = dto.User.Email,
                    //Address = dto.User.Address,
                    Rating = 0,
                    ReviewCount = 0,
                    CreateAt = DateTime.Now
                };

                _db.User.Add(user);

                // 🔥 3. 建立 User
                var user_habit = new User_Habit {
                    User = user,
                    SleepTime = new TimeOnly(22, 0),
                    WakeTime = new TimeOnly(6, 0),
                    CleanLevel = 0,
                    NoiseTolerance = 0,
                    Pet = false,
                    Smoke = false,
                    Interests = ""
                };

                _db.User_Habit.Add(user_habit);

                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception err) {
                await transaction.RollbackAsync();
                return StatusCode(500, new {
                    message = "註冊失敗",
                    error = err
                });
            }

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
