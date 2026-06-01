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
        // 1. 承租人送出預約申請
        // ===================================================================
        [HttpPost("apply")]
        public async Task<IActionResult> CreateApplication([FromBody] CreateViewingOrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 自動產生專題規格的預約單號
            string uniqueId = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
            string generatedOrderNumber = $"B-{DateTime.Now:yyyyMMdd}-{uniqueId}";

            // 建立要存入資料庫的 Entity 物件
            var newViewing = new HouseViewing
            {
                ReservationNo = generatedOrderNumber, // ⭕ 已修正為 ReservationNo
                HouseId = dto.HouseId,
                LesseId = dto.LesseeId,               // ⭕ 已修正為 LesseId (單個 e)
                LessorId = dto.LessorId,
                ViewingTime = dto.ViewingTime,        // ⭕ 已修正為 ViewingTime
                ExpectedMoveIn = dto.ExpectedMoveIn,
                Message = dto.Message,
                MatchScore = dto.MatchScore,
                Status = 0,                           // 🚨 已修正：資料庫是 int，0 代表 pending
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            try
            {
                _context.HouseViewings.Add(newViewing);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "預約申請已成功送出！", orderNumber = generatedOrderNumber });
            }
            catch (Exception ex)
            {
                // 這裡如果出錯，會把最底層的錯誤細節吐出來，方便你除錯
                return StatusCode(500, new { message = "送出預約失敗，請稍後再試", details = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // ===================================================================
        // 2. 出租人(房東)取得「看房預約審核」列表
        // ===================================================================
        [HttpGet("lessor/{lessorId}/approvals")]
        public async Task<IActionResult> GetLessorApprovals(int lessorId)
        {
            try
            {
                var approvals = await _context.HouseViewings
                    .Where(v => v.LessorId == lessorId)
                    .OrderByDescending(v => v.CreatedAt)
                    .Select(v => new ViewingOrderResponseDto
                    {
                        Id = v.Id.ToString(),
                        OrderNumber = v.ReservationNo, // ⭕ 已修正為 ReservationNo

                        // 🚨 轉型處理：因為前端預期收到字串狀態，我們在這裡把 int 轉回字串
                        Status = v.Status == 1 ? "confirmed" : v.Status == 2 ? "rejected" : "pending",

                        RoomName = v.RentHouse != null ? v.RentHouse.Name : "未知房源",
                        Applicant = new ApplicantDetailDto
                        {
                            Name = "申請人_" + v.LesseId, // ⭕ 已修正為 LesseId
                            Avatar = "images/mr_chen.jpg",
                            Profiles = new List<string> { "單人", "無寵", "不菸" },
                            MoveInDate = v.ExpectedMoveIn.ToString("yyyy/MM/dd"),
                            Phone = "0912***678",
                            LineId = "line_" + v.LesseId
                        },
                        ViewingDateTime = v.ViewingTime.ToString("yyyy/MM/dd (dd) HH:mm"), // ⭕ 已修正為 ViewingTime
                        Message = v.Message,
                        MatchScore = v.MatchScore
                    })
                    .ToListAsync();

                return Ok(approvals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得審核列表失敗", details = ex.InnerException?.Message ?? ex.Message });
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
                    .Where(v => v.LesseId == lesseeId) // 💡 如果資料庫是 LesseId，這行稍後編譯若報錯請改為 v.LesseId == lesseeId
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
                            MoveInDate = v.ExpectedMoveIn.ToString("yyyy/MM/dd"),
                            Phone = "0912***678",
                            LineId = "my_line_id"
                        },
                        ViewingDateTime = v.ViewingTime.ToString("yyyy/MM/dd (dd) HH:mm"), // ⭕ 已修正為 ViewingTime
                        Message = v.Message,
                        MatchScore = v.MatchScore
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