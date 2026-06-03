using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentApi.Data;
using RentApi.Models;
using RentApi.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HouseViewingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HouseViewingController(AppDbContext context)
        {
            _context = context;
        }

        // ===================================================================
        // 1. 承租人送出預約申請 (對應詳細頁面的「確認預約」)
        // ===================================================================
        // POST: api/HouseViewing/apply
        [HttpPost("apply")]
        public async Task<IActionResult> CreateApplication([FromBody] CreateViewingOrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.HouseId <= 0)
            {
                return BadRequest(new
                {
                    message = "HouseId 不可為 0，請檢查前端是否有送 houseId，或 DTO 的 JsonPropertyName 是否設定錯誤。"
                });
            }

            // 自動產生專題規格的預約單號 (例如: B-20260601-A3D2)
            string uniqueId = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
            string generatedOrderNumber = $"B-{DateTime.Now:yyyyMMdd}-{uniqueId}";

            try
            {
                // 💡 【新加入的防呆與查詢邏輯】
                // 1. 先用前端傳來的 HouseId，去房屋表找出這間房子
                // (請根據你的 AppDbContext 調整 _context.Rent_Houses 的名稱，有時可能是小寫或單數)
                var house = await _context.Rent_Houses.FirstOrDefaultAsync(h => h.Id == dto.HouseId);
                if (house == null)
                {
                    return BadRequest(new { message = $"找不到 ID 為 {dto.HouseId} 的房屋資料" });
                }

                // 2. 拿房屋表裡面的 AccountId，去 User 表找出這個房東真實的 User.Id
                // (請根據你的 AppDbContext 調整 _context.Users 或 _context.User 的名稱)
                var lessorUser = await _context.User.FirstOrDefaultAsync(u => u.AccountId == house.AccountId);
                if (lessorUser == null)
                {
                    return BadRequest(new { message = $"找不到該房屋(AccountId: {house.AccountId})對應的房東使用者帳戶" });
                }

                // 建立要存入資料庫的 Entity 物件
                var newViewing = new HouseViewing
                {
                    ReservationNo = generatedOrderNumber,
                    HouseId = dto.HouseId,

                    LesseeId = dto.LesseeId,
                    LessorId = lessorUser.Id,

                    //ViewingTime = DateTime.Parse(dto.ViewingTime),
                    //ExpectedMoveIn = DateTime.Parse(dto.ExpectedMoveIn),

                    ViewingTime = dto.ViewingTime,
                    ExpectedMoveIn = dto.ExpectedMoveIn,
                    Message = dto.Message,
                    MatchScore = dto.MatchScore,
                    Status = 0,                // 預設為 0 (pending)
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.HouseViewings.Add(newViewing);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "預約申請已成功送出！", orderNumber = generatedOrderNumber });
            }
            catch (Exception ex)
            {
                // 這裡回傳 InnerException，如果資料庫又報錯，前端能看到最底層的 SQL 錯誤原因
                return StatusCode(500, new { message = "送出預約失敗，請稍後再試", details = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // ===================================================================
        // 2. 出租人(房東)取得「看房預約審核」列表
        // ===================================================================
        [HttpGet("lessor/{lessorId:int}/approvals")]
        public async Task<IActionResult> GetLessorApprovals(int lessorId)
        {
            try
            {
                var rawData = await (
                    from v in _context.HouseViewings.AsNoTracking()
                    join h in _context.Rent_Houses.AsNoTracking()
                        on v.HouseId equals h.Id into houseJoin
                    from h in houseJoin.DefaultIfEmpty()
                    where v.LessorId == lessorId
                    orderby ((DateTime?)v.CreatedAt ?? DateTime.MinValue) descending
                    select new
                    {
                        Id = v.Id,

                        ReservationNo = v.ReservationNo,

                        Status = (int?)v.Status,

                        RoomName = h != null ? h.Name : null,

                        LesseeId = (int?)v.LesseeId,

                        ExpectedMoveIn = (DateTime?)v.ExpectedMoveIn,

                        ViewingTime = (DateTime?)v.ViewingTime,

                        Message = v.Message,

                        MatchScore = (int?)v.MatchScore
                    }
                ).ToListAsync();

                var approvals = rawData.Select(v => new ViewingOrderResponseDto
                {
                    Id = v.Id.ToString(),

                    OrderNumber = string.IsNullOrWhiteSpace(v.ReservationNo)
                        ? $"B-FIX-{v.Id}"
                        : v.ReservationNo,

                    Status = v.Status == 1 ? "confirmed" :
                             v.Status == 2 ? "rejected" :
                             "pending",

                    RoomName = string.IsNullOrWhiteSpace(v.RoomName)
                        ? "未知房源"
                        : v.RoomName,

                    Applicant = new ApplicantDetailDto
                    {
                        Name = $"申請人_{v.LesseeId ?? 0}",
                        Avatar = "images/mr_chen.jpg",
                        Profiles = new List<string> { "單人", "無寵", "不菸" },

                        MoveInDate = v.ExpectedMoveIn.HasValue
                            ? v.ExpectedMoveIn.Value.ToString("yyyy/MM/dd")
                            : "尚未填寫",

                        Phone = "0912***678",
                        LineId = $"line_{v.LesseeId ?? 0}"
                    },

                    ViewingDateTime = v.ViewingTime.HasValue
                        ? v.ViewingTime.Value.ToString("yyyy/MM/dd HH:mm")
                        : "尚未選擇時間",

                    Message = string.IsNullOrWhiteSpace(v.Message)
                        ? "無留言"
                        : v.Message,

                    MatchScore = v.MatchScore ?? 0
                }).ToList();

                return Ok(approvals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "取得審核列表失敗",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // ===================================================================
        // 3. 承租人取得「看房申請追蹤」列表
        // ===================================================================
        [HttpGet("lessee/{lesseeId}/applications")]
        public async Task<IActionResult> GetLesseeApplications(int lesseeId)
        {
            try
            {
                var applications = await _context.HouseViewings
                    .Where(v => v.LesseeId == lesseeId) // 💡 如果資料庫是 LesseId，這行稍後編譯若報錯請改為 v.LesseId == lesseeId
                    .OrderByDescending(v => v.CreatedAt)
                    .Select(v => new ViewingOrderResponseDto
                    {
                        Id = v.Id.ToString(),
                        OrderNumber = v.ReservationNo, // ⭕ 已修正為 ReservationNo
                        Status = v.Status == 1 ? "confirmed" : v.Status == 2 ? "rejected" : "pending",
                        RoomName = v.RentHouse != null ? v.RentHouse.Name : "未知房源",
                        Applicant = new ApplicantDetailDto
                        {
                            Name = "我自己",
                            Avatar = "images/mr_chen.jpg",
                            Profiles = new List<string> { "單人", "無寵", "不菸" },
                            MoveInDate = v.ExpectedMoveIn.HasValue
                                ? v.ExpectedMoveIn.Value.ToString("yyyy/MM/dd")
                                : "尚未填寫",
                            Phone = "0912***678",
                            LineId = "my_line_id"
                        },
                        ViewingDateTime = v.ViewingTime.HasValue
                            ? v.ViewingTime.Value.ToString("yyyy/MM/dd HH:mm")
                            : "尚未選擇時間",
                        Message = v.Message,
                        MatchScore = v.MatchScore ?? 0
                    })
                    .ToListAsync();

                return Ok(applications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得追蹤列表失敗", details = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // ===================================================================
        // 4. 出租人(房東)更新預約狀態
        // ===================================================================
        [HttpPatch("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateReservationStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reservation = await _context.HouseViewings
                                            .FirstOrDefaultAsync(v => v.Id == dto.ReservationId);

            if (reservation == null)
            {
                return NotFound(new { message = "找不到該筆預約單" });
            }

            // 🚨 轉型處理：前端傳來 "confirmed" 轉成 1，"rejected" 轉成 2，其餘為 0
            reservation.Status = dto.Status == "confirmed" ? 1 : dto.Status == "rejected" ? 2 : 0;
            reservation.UpdatedAt = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"預約單 {dto.ReservationId} 狀態已成功更新",
                    updatedId = reservation.Id,
                    newStatus = dto.Status
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "資料庫更新失敗", details = ex.InnerException?.Message ?? ex.Message });
            }
        }
    }
}