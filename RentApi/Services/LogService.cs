using Microsoft.EntityFrameworkCore;
using RentApi.Data;
using RentApi.Models;
using RentApi.Models.DTO;

namespace RentApi.Services {
    public class LogService {
        private readonly AppDbContext _db;

        public LogService(AppDbContext db) {
            _db = db;
        }
        public async Task<List<GetSystem_LogDto>?> GetAllAsync() {
            var res = await (
                from log in _db.System_Log
                join admin in _db.Admin
                    on log.UserId equals admin.Id
                select new GetSystem_LogDto {
                    Id = log.Id,
                    Name = admin.Username,
                    Action = log.Action,
                    IpAddress = log.IpAddress,
                    CreatedAt = log.CreatedAt,
                }
            ).ToListAsync();

            return res;
        }

        public async Task<List<System_Log>?> GetByUserIdAsync(int id) {
            var res = await _db.System_Log.Where(x => x.UserId == id).ToListAsync();
            return res;
        }

        public async Task<System_Log?> PostAsync(System_LogDto log) {
            string? AdminName = _db.Admin.Where(x => x.Id == log.UserId).Select(x => x.Username).ToString();
            var res = new System_Log {
                UserId = log.UserId,
                Action = log.Action,
                IpAddress = log.IpAddress,
                CreatedAt = DateTime.Now
            };
            
            _db.System_Log.Add(res);
            await _db.SaveChangesAsync();
            return res;
        }
    }
}
