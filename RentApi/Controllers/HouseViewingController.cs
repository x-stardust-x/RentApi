using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentApi.Data;
using RentApi.Models;
using RentApi.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;


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

        [Authorize]
        [HttpPost("apply")]
        public async Task<IActionResult> CreateApplication([FromBody] CreateViewingOrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await GetCurrentUserAsync();

            if (currentUser == null)
            {
                return Unauthorized(new { message = "請先登入後再送出預約" });
            }

            string uniqueId = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
            string generatedOrderNumber = $"B-{DateTime.Now:yyyyMMdd}-{uniqueId}";

            try
            {
                var house = await _context.Rent_Houses.FirstOrDefaultAsync(h => h.Id == dto.HouseId);

                if (house == null)
                {
                    return BadRequest(new { message = $"找不到 ID 為 {dto.HouseId} 的房屋資料" });
                }

                if (!house.IsVisible || house.RentalStatus == "matched")
                {
                    return BadRequest(new { message = "此房源已媒合或已下架，無法再送出看房申請" });
                }

                //var lessorUser = await _context.User.FirstOrDefaultAsync(u => u.AccountId == house.AccountId);

                var lessorUser = await _context.User
                    .FirstOrDefaultAsync(u => u.AccountId == house.AccountId);

                if (lessorUser == null)
                {
                    return BadRequest(new { message = $"找不到該房屋(AccountId: {house.AccountId})對應的出租人帳號" });
                }

                if (lessorUser.Id == currentUser.Id)
                {
                    return BadRequest(new { message = "不能預約自己發布的房源" });
                }

                DateTime? finalViewingTime = dto.ViewingTime;

                if (dto.ViewingSlotId.HasValue)
                {
                    var selectedSlot = await _context.HouseViewingAvailableSlots
                        .FirstOrDefaultAsync(s =>
                            s.Id == dto.ViewingSlotId.Value &&
                            s.HouseId == dto.HouseId &&
                            s.IsEnabled);

                    if (selectedSlot == null)
                    {
                        return BadRequest(new { message = "選擇的看房時段不存在或已停用" });
                    }

                    if (selectedSlot.AvailableDate.HasValue)
                    {
                        finalViewingTime = selectedSlot.AvailableDate.Value.ToDateTime(selectedSlot.StartTime);
                    }
                    else if (!dto.ViewingTime.HasValue)
                    {
                        return BadRequest(new { message = "請選擇看房日期" });
                    }
                }

                var newViewing = new HouseViewing
                {
                    ReservationNo = generatedOrderNumber,
                    HouseId = dto.HouseId,

                    LesseeId = currentUser.Id,
                    LessorId = lessorUser.Id,

                    ViewingSlotId = dto.ViewingSlotId,
                    ViewingTime = finalViewingTime ?? DateTime.Now,
                    ExpectedMoveIn = dto.ExpectedMoveIn ?? DateTime.Now,
                    ExpectedMoveInText = dto.ExpectedMoveInText,

                    PreferredTimeSlotsJson = dto.PreferredTimeSlots == null
                        ? null
                        : JsonSerializer.Serialize(dto.PreferredTimeSlots),

                    LesseeProfileTagsJson = dto.LesseeProfileTags == null
                        ? null
                        : JsonSerializer.Serialize(dto.LesseeProfileTags),

                    Message = dto.Message ?? string.Empty,
                    MatchScore = dto.MatchScore,
                    Status = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.HouseViewings.Add(newViewing);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "預約申請已成功送出！",
                    orderNumber = generatedOrderNumber
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "送出預約失敗，請稍後再試",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }



        //[HttpPost("apply")]
        //public async Task<IActionResult> CreateApplication([FromBody] CreateViewingOrderDto dto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (dto.HouseId <= 0)
        //    {
        //        return BadRequest(new
        //        {
        //            message = "HouseId 不可為 0，請檢查前端是否有送 houseId，或 DTO 的 JsonPropertyName 是否設定錯誤。"
        //        });
        //    }

        //    // 自動產生專題規格的預約單號 (例如: B-20260601-A3D2)
        //    string uniqueId = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
        //    string generatedOrderNumber = $"B-{DateTime.Now:yyyyMMdd}-{uniqueId}";

        //    try
        //    {
        //        // 💡 【新加入的防呆與查詢邏輯】
        //        // 1. 先用前端傳來的 HouseId，去房屋表找出這間房子
        //        // (請根據你的 AppDbContext 調整 _context.Rent_Houses 的名稱，有時可能是小寫或單數)
        //        var house = await _context.Rent_Houses.FirstOrDefaultAsync(h => h.Id == dto.HouseId);
        //        if (house == null)
        //        {
        //            return BadRequest(new { message = $"找不到 ID 為 {dto.HouseId} 的房屋資料" });
        //        }

        //        // 2. 拿房屋表裡面的 AccountId，去 User 表找出這個房東真實的 User.Id
        //        // (請根據你的 AppDbContext 調整 _context.Users 或 _context.User 的名稱)
        //        var lessorUser = await _context.User.FirstOrDefaultAsync(u => u.AccountId == house.AccountId);
        //        if (lessorUser == null)
        //        {
        //            return BadRequest(new { message = $"找不到該房屋(AccountId: {house.AccountId})對應的房東使用者帳戶" });
        //        }

        //        // 建立要存入資料庫的 Entity 物件
        //        var newViewing = new HouseViewing
        //        {
        //            ReservationNo = generatedOrderNumber,
        //            HouseId = dto.HouseId,

        //            // 之後建議改成 currentUser.Id，不要相信前端傳 LesseeId
        //            LesseeId = currentUser.Id,
        //            LessorId = lessorUser.Id,

        //            ViewingSlotId = dto.ViewingSlotId,

        //            ViewingTime = dto.ViewingTime,
        //            ExpectedMoveIn = dto.ExpectedMoveIn ?? DateTime.Now,
        //            ExpectedMoveInText = dto.ExpectedMoveInText,

        //            PreferredTimeSlotsJson = dto.PreferredTimeSlots == null
        //                ? null
        //                : JsonSerializer.Serialize(dto.PreferredTimeSlots),

        //            LesseeProfileTagsJson = dto.LesseeProfileTags == null
        //                ? null
        //                : JsonSerializer.Serialize(dto.LesseeProfileTags),

        //            Message = dto.Message ?? string.Empty,
        //            MatchScore = dto.MatchScore,
        //            Status = 0,
        //            CreatedAt = DateTime.Now,
        //            UpdatedAt = DateTime.Now
        //        };

        //        _context.HouseViewings.Add(newViewing);
        //        await _context.SaveChangesAsync();

        //        return Ok(new { success = true, message = "預約申請已成功送出！", orderNumber = generatedOrderNumber });
        //    }
        //    catch (Exception ex)
        //    {
        //        // 這裡回傳 InnerException，如果資料庫又報錯，前端能看到最底層的 SQL 錯誤原因
        //        return StatusCode(500, new { message = "送出預約失敗，請稍後再試", details = ex.InnerException?.Message ?? ex.Message });
        //    }
        //}

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

                        MatchScore = (int?)v.MatchScore,
                        v.MatchedAt,
                        v.MatchNote,
                        v.ClosedReason,
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

                    MatchScore = v.MatchScore ?? 0,
                    MatchedAt = v.MatchedAt,
                    MatchNote = v.MatchNote ?? "",
                    ClosedReason = v.ClosedReason ?? "",
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

        private async Task<User?> GetCurrentUserAsync()
        {
            var rawId =
                User.FindFirstValue("UserId") ??
                User.FindFirstValue("userId") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(rawId, out var id))
            {
                return null;
            }

            var userByUserId = await _context.User.FirstOrDefaultAsync(u => u.Id == id);
            if (userByUserId != null) return userByUserId;

            var userByAccountId = await _context.User.FirstOrDefaultAsync(u => u.AccountId == id);
            return userByAccountId;
        }


        [Authorize]
        [HttpGet("my-lessee-profile-tags")]
        public async Task<IActionResult> GetMyLesseeProfileTags()
        {
            var currentUser = await GetCurrentUserAsync();

            if (currentUser == null)
            {
                return Unauthorized(new { message = "請先登入" });
            }

            var habit = await _context.User_Habit
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.UserId == currentUser.Id);

            var tags = new List<object>();

            if (habit == null)
            {
                return Ok(tags);
            }

            if (habit.CleanLevel >= 4)
                tags.Add(new { label = "重視整潔", source = "habit", icon = "cleaning_services" });
            else if (habit.CleanLevel <= 2)
                tags.Add(new { label = "可接受生活感", source = "habit", icon = "home" });
            else
                tags.Add(new { label = "普通整潔", source = "habit", icon = "mop" });

            if (habit.NoiseTolerance <= 2)
                tags.Add(new { label = "喜歡安靜", source = "habit", icon = "volume_off" });
            else if (habit.NoiseTolerance >= 4)
                tags.Add(new { label = "可接受熱鬧", source = "habit", icon = "groups" });
            else
                tags.Add(new { label = "一般音量可接受", source = "habit", icon = "volume_up" });

            tags.Add(new
            {
                label = habit.Pet == true ? "可接受寵物" : "無寵物",
                source = "habit",
                icon = "pets"
            });

            tags.Add(new
            {
                label = habit.Smoke == true ? "可接受抽菸" : "不抽菸",
                source = "habit",
                icon = "smoke_free"
            });

            if (!string.IsNullOrWhiteSpace(habit.Interests))
            {
                var interests = habit.Interests
                    .Split(new[] { ',', '，', ';', '；' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0);

                foreach (var interest in interests)
                {
                    tags.Add(new { label = interest, source = "habit", icon = "interests" });
                }
            }

            return Ok(tags);
        }

        [Authorize]
        [HttpPut("house/{houseId:int}/available-slots")]
        public async Task<IActionResult> ReplaceAvailableSlotsByHouse(
            int houseId,
            [FromBody] List<UpsertViewingSlotDto> request
        )
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();

                if (currentUser == null)
                {
                    return Unauthorized(new { message = "請先登入" });
                }

                //var house = await _context.Rent_Houses
                //    .FirstOrDefaultAsync(h => h.Id == dto.HouseId);

                var house = await _context.Rent_Houses
                    .FirstOrDefaultAsync(h => h.Id == houseId);

                if (house == null)
                {
                    return NotFound(new { message = "找不到房源資料" });
                }
                          

                var lessorUser = await _context.User
                    .FirstOrDefaultAsync(u => u.AccountId == house.AccountId);

                if (lessorUser == null)
                {
                    return BadRequest(new { message = $"找不到此房源對應的出租人資料，Rent_House.AccountId = {house.AccountId}" });
                }

                var oldSlots = await _context.HouseViewingAvailableSlots
                    .Where(s => s.HouseId == houseId)
                    .ToListAsync();

                foreach (var oldSlot in oldSlots)
                {
                    oldSlot.IsEnabled = false;
                    oldSlot.UpdatedAt = DateTime.Now;
                }

                foreach (var item in request)
                {
                    if (string.IsNullOrWhiteSpace(item.StartTime) ||
                        string.IsNullOrWhiteSpace(item.EndTime))
                    {
                        continue;
                    }

                    var startTime = TimeOnly.Parse(item.StartTime);
                    var endTime = TimeOnly.Parse(item.EndTime);

                    if (endTime <= startTime)
                    {
                        return BadRequest(new { message = "結束時間必須晚於開始時間" });
                    }

                    _context.HouseViewingAvailableSlots.Add(new HouseViewingAvailableSlot
                    {
                        HouseId = houseId,
                        LessorId = lessorUser.Id,
                        AvailableDate = null,
                        StartTime = startTime,
                        EndTime = endTime,
                        IsEnabled = item.IsEnabled,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "房源可看房時段已更新" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "更新可看房時段失敗",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }


        [HttpGet("house/{houseId:int}/available-slots")]
        public async Task<IActionResult> GetAvailableSlotsByHouse(int houseId)
        {
            try
            {
                var rawSlots = await _context.HouseViewingAvailableSlots
                    .AsNoTracking()
                    .Where(s => s.HouseId == houseId && s.IsEnabled)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();

                var slots = rawSlots.Select(s => new
                {
                    id = s.Id,
                    houseId = s.HouseId,
                    lessorId = s.LessorId,
                    availableDate = s.AvailableDate.HasValue
                        ? s.AvailableDate.Value.ToString("yyyy-MM-dd")
                        : null,
                    startTime = s.StartTime.ToString("HH:mm"),
                    endTime = s.EndTime.ToString("HH:mm"),
                    label = $"{s.StartTime:HH:mm} - {s.EndTime:HH:mm}",
                    isEnabled = s.IsEnabled
                }).ToList();

                return Ok(slots);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "取得可預約時段失敗",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }


        [Authorize]
        [HttpGet("my-approvals")]
        public async Task<IActionResult> GetMyApprovals()
        {
            var currentUser = await GetCurrentUserAsync();

            if (currentUser == null)
            {
                return Unauthorized(new { message = "請先登入" });
            }

            try
            {
                var rawData = await (
                    from v in _context.HouseViewings.AsNoTracking()

                    join h in _context.Rent_Houses.AsNoTracking()
                        on v.HouseId equals h.Id into houseJoin
                    from h in houseJoin.DefaultIfEmpty()

                    join lessee in _context.User.AsNoTracking()
                        on v.LesseeId equals lessee.Id into lesseeJoin
                    from lessee in lesseeJoin.DefaultIfEmpty()

                    join lesseeAccount in _context.Account.AsNoTracking()
                        on lessee.AccountId equals lesseeAccount.Id into lesseeAccountJoin
                    from lesseeAccount in lesseeAccountJoin.DefaultIfEmpty()

                    where v.LessorId == currentUser.Id

                    orderby v.CreatedAt descending

                    select new
                    {
                        v.Id,
                        v.ReservationNo,
                        v.Status,

                        RoomName = h != null ? h.Name : "未知房源",
                        RoomAddress = h != null ? h.Address : "",

                        LesseeName =
                            lessee != null && !string.IsNullOrWhiteSpace(lessee.RealName)
                                ? lessee.RealName
                                : lesseeAccount != null && !string.IsNullOrWhiteSpace(lesseeAccount.Username)
                                    ? lesseeAccount.Username
                                    : "未知申請人",
                        LesseePhone = lessee != null ? lessee.Phone : "",
                        LesseeLineId = lessee != null ? lessee.LineId : "",
                        LesseeAvatar = lessee != null ? lessee.Avatar : "",

                        v.ViewingTime,
                        v.ExpectedMoveIn,
                        v.ExpectedMoveInText,
                        v.PreferredTimeSlotsJson,
                        v.LesseeProfileTagsJson,
                        v.Message,
                        v.MatchScore,

                        v.RescheduleProposedTime,
                        v.RescheduleEndTime,
                        v.RescheduleMessage,
                        v.RescheduleCount,

                        v.ApplicationFlowType,
                        v.AttemptNo,
                        v.MaxAttemptCount,

                        v.MatchedAt,
                        v.MatchNote,
                        v.ClosedReason
                    }
                ).ToListAsync();

                var approvals = rawData.Select(v => new ViewingOrderResponseDto
                {
                    Id = v.Id.ToString(),

                    OrderNumber = string.IsNullOrWhiteSpace(v.ReservationNo)
                        ? $"B-FIX-{v.Id}"
                        : v.ReservationNo,

                    Status = (v.Status ?? 0) == 1 ? "confirmed" :
                        (v.Status ?? 0) == 2 ? "rejected" :
                        (v.Status ?? 0) == 3 ? "rescheduled" :
                        (v.Status ?? 0) == 4 ? "matched" :
                        (v.Status ?? 0) == 5 ? "closed" :
                        "pending",

                    RoomName = string.IsNullOrWhiteSpace(v.RoomName)
                        ? "未知房源"
                        : v.RoomName,

                    RoomAddress = string.IsNullOrWhiteSpace(v.RoomAddress)
                        ? ""
                        : v.RoomAddress,

                    Applicant = new ApplicantDetailDto
                    {
                        Name = string.IsNullOrWhiteSpace(v.LesseeName)
                            ? "未知申請人"
                            : v.LesseeName,

                        Avatar = string.IsNullOrWhiteSpace(v.LesseeAvatar)
                            ? "images/mr_chen.jpg"
                            : v.LesseeAvatar,

                        Profiles = ParseLesseeProfileLabels(v.LesseeProfileTagsJson),

                        MoveInDate = !string.IsNullOrWhiteSpace(v.ExpectedMoveInText)
                            ? v.ExpectedMoveInText
                            : v.ExpectedMoveIn.HasValue
                                ? v.ExpectedMoveIn.Value.ToString("yyyy/MM/dd")
                                : "尚未填寫",

                        Phone = string.IsNullOrWhiteSpace(v.LesseePhone)
                            ? "未填寫"
                            : v.LesseePhone,

                        LineId = string.IsNullOrWhiteSpace(v.LesseeLineId)
                            ? "未填寫"
                            : v.LesseeLineId
                    },

                    ViewingDateTime = v.ViewingTime.HasValue
                        ? v.ViewingTime.Value.ToString("yyyy/MM/dd HH:mm")
                        : "尚未選擇時間",

                    ViewingDate = v.ViewingTime.HasValue
                        ? v.ViewingTime.Value.ToString("yyyy/MM/dd")
                        : "尚未選擇日期",

                    PreferredTimeSlots = ParsePreferredTimeSlots(v.PreferredTimeSlotsJson),

                    Message = string.IsNullOrWhiteSpace(v.Message)
                        ? "無留言"
                        : v.Message,

                    MatchScore = v.MatchScore ?? 0,


                    ApplicationFlowType = string.IsNullOrWhiteSpace(v.ApplicationFlowType)
                        ? "new"
                        : v.ApplicationFlowType,

                    AttemptNo = v.AttemptNo <= 0 ? 1 : v.AttemptNo,

                    MaxAttemptCount = v.MaxAttemptCount <= 0 ? 3 : v.MaxAttemptCount,

                    RescheduleInfo = v.RescheduleProposedTime.HasValue
                        ? new RescheduleInfoDto
                        {
                            ProposedViewingDateTime =
                                v.RescheduleEndTime.HasValue
                                    ? $"{v.RescheduleProposedTime.Value:yyyy/MM/dd HH:mm} - {v.RescheduleEndTime.Value:HH:mm}"
                                    : $"{v.RescheduleProposedTime.Value:yyyy/MM/dd HH:mm}",

                            Message = v.RescheduleMessage ?? "",
                            Count = v.RescheduleCount
                        }
                        : null,



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

        [Authorize]
        [HttpGet("my-applications")]
        public async Task<IActionResult> GetMyApplications()
        {
            var currentUser = await GetCurrentUserAsync();

            if (currentUser == null)
            {
                return Unauthorized(new { message = "請先登入" });
            }

            try
            {
                var rawData = await (
                    from v in _context.HouseViewings.AsNoTracking()

                    join h in _context.Rent_Houses.AsNoTracking()
                        on v.HouseId equals h.Id into houseJoin
                    from h in houseJoin.DefaultIfEmpty()

                    join lessor in _context.User.AsNoTracking()
                        on v.LessorId equals lessor.Id into lessorJoin
                    from lessor in lessorJoin.DefaultIfEmpty()

                    where v.LesseeId == currentUser.Id

                    orderby v.CreatedAt descending

                    select new
                    {
                        HouseId = v.HouseId,

                        v.Id,
                        v.ReservationNo,
                        v.Status,
                        v.RejectReason,

                        RoomName = h != null ? h.Name : "未知房源",
                        RoomAddress = h != null ? h.Address : "",
                        RentPrice = h != null ? h.RentPrice : 0,

                        CoverUrl = _context.House_Images
                            .AsNoTracking()
                            .Where(img => img.HouseId == v.HouseId)
                            .OrderByDescending(img => img.IsCover == true)
                            .Select(img => img.Url)
                            .FirstOrDefault(),

                        LessorId = lessor != null ? lessor.Id : 0,
                        LessorAccountId = lessor != null ? (int?)lessor.AccountId : null,

                        LessorName = lessor != null && !string.IsNullOrWhiteSpace(lessor.RealName)
                            ? lessor.RealName
                            : "未知出租人",

                        LessorAvatar = lessor != null ? lessor.Avatar : "",
                        LessorPhone = lessor != null ? lessor.Phone : "",
                        LessorLineId = lessor != null ? lessor.LineId : "",

                        v.ViewingTime,
                        v.ExpectedMoveIn,
                        v.ExpectedMoveInText,
                        v.PreferredTimeSlotsJson,
                        v.LesseeProfileTagsJson,
                        v.Message,
                        v.MatchScore,

                        v.RescheduleProposedTime,
                        v.RescheduleEndTime,
                        v.RescheduleMessage,
                        v.RescheduleCount,

                        v.ApplicationFlowType,
                        v.AttemptNo,
                        v.MaxAttemptCount,

                        v.MatchedAt,
                        v.MatchNote,
                        v.ClosedReason
                    }
                ).ToListAsync();

                var applications = rawData.Select(v => new LesseeViewingApplicationDto
                {
                    Id = v.Id.ToString(),

                    OrderNumber = string.IsNullOrWhiteSpace(v.ReservationNo)
        ? $"B-FIX-{v.Id}"
        : v.ReservationNo,

                    Status = (v.Status ?? 0) == 1 ? "confirmed" :
             (v.Status ?? 0) == 2 ? "rejected" :
             (v.Status ?? 0) == 3 ? "rescheduled" :
             (v.Status ?? 0) == 4 ? "matched" :
             (v.Status ?? 0) == 5 ? "closed" :
             "pending",

                    ApplicationFlowType = string.IsNullOrWhiteSpace(v.ApplicationFlowType)
        ? "new"
        : v.ApplicationFlowType,

                    AttemptNo = v.AttemptNo <= 0 ? 1 : v.AttemptNo,

                    MaxAttemptCount = v.MaxAttemptCount <= 0 ? 3 : v.MaxAttemptCount,

                    MatchedAt = v.MatchedAt,
                    MatchNote = v.MatchNote ?? "",
                    ClosedReason = v.ClosedReason ?? "",

                    HouseId = v.HouseId,

                    RoomName = string.IsNullOrWhiteSpace(v.RoomName)
        ? "未知房源"
        : v.RoomName,

                    RoomAddress = string.IsNullOrWhiteSpace(v.RoomAddress)
        ? "尚未提供地址"
        : v.RoomAddress,

                    CoverUrl = string.IsNullOrWhiteSpace(v.CoverUrl)
        ? ""
        : v.CoverUrl,

                    RentPrice = v.RentPrice,

                    LessorId = v.LessorId,
                    LessorAccountId = v.LessorAccountId,
                    LessorProfileId = v.LessorAccountId ?? v.LessorId,

                    LessorName = string.IsNullOrWhiteSpace(v.LessorName)
        ? "未知出租人"
        : v.LessorName,

                    LessorAvatar = string.IsNullOrWhiteSpace(v.LessorAvatar)
        ? ""
        : v.LessorAvatar,

                    LessorPhone = string.IsNullOrWhiteSpace(v.LessorPhone)
        ? "未填寫"
        : v.LessorPhone,

                    LessorLineId = string.IsNullOrWhiteSpace(v.LessorLineId)
        ? "未填寫"
        : v.LessorLineId,

                    ViewingDate = v.ViewingTime.HasValue
        ? v.ViewingTime.Value.ToString("yyyy/MM/dd")
        : "尚未選擇日期",

                    ViewingDateTime = v.ViewingTime.HasValue
        ? v.ViewingTime.Value.ToString("yyyy/MM/dd HH:mm")
        : "尚未選擇時間",

                    PreferredTimeSlots = ParsePreferredTimeSlots(v.PreferredTimeSlotsJson),

                    ExpectedMoveInText = !string.IsNullOrWhiteSpace(v.ExpectedMoveInText)
        ? v.ExpectedMoveInText
        : v.ExpectedMoveIn.HasValue
            ? v.ExpectedMoveIn.Value.ToString("yyyy/MM/dd")
            : "尚未填寫",

                    LesseeProfileTags = ParseLesseeProfileTags(v.LesseeProfileTagsJson),

                    Message = string.IsNullOrWhiteSpace(v.Message)
        ? "無留言"
        : v.Message,

                    MatchScore = v.MatchScore ?? 0,

                    RejectReason = string.IsNullOrWhiteSpace(v.RejectReason)
        ? ""
        : v.RejectReason,

                    RescheduleInfo = v.RescheduleProposedTime.HasValue
        ? new RescheduleInfoDto
        {
            ProposedViewingDateTime =
                v.RescheduleEndTime.HasValue
                    ? $"{v.RescheduleProposedTime.Value:yyyy/MM/dd HH:mm} - {v.RescheduleEndTime.Value:HH:mm}"
                    : $"{v.RescheduleProposedTime.Value:yyyy/MM/dd HH:mm}",

            Message = v.RescheduleMessage ?? "",
            Count = v.RescheduleCount
        }
        : null
                }).ToList();

                return Ok(applications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "取得看房申請追蹤失敗",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        private static List<string> ParseLesseeProfileLabels(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<string>();
            }

            try
            {
                var tags = JsonSerializer.Deserialize<List<LesseeProfileTagDto>>(json);
                return tags?.Select(t => t.Label).Where(x => !string.IsNullOrWhiteSpace(x)).ToList()
                       ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static string BuildViewingMessage(string? preferredTimeSlotsJson, string? message)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(preferredTimeSlotsJson))
            {
                try
                {
                    var slots = JsonSerializer.Deserialize<List<string>>(preferredTimeSlotsJson);

                    if (slots != null && slots.Any())
                    {
                        parts.Add("期望時段：" + string.Join("、", slots));
                    }
                }
                catch
                {
                    parts.Add("期望時段：" + preferredTimeSlotsJson);
                }
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                parts.Add("留言：" + message);
            }

            return parts.Count == 0 ? "無留言" : string.Join("｜", parts);
        }

        private static List<string> ParsePreferredTimeSlots(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<string>();
            }

            try
            {
                var slots = JsonSerializer.Deserialize<List<string>>(json);

                return slots?
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList()
                    ?? new List<string>();
            }
            catch
            {
                return new List<string> { json };
            }
        }

        // 接受 / 婉拒預約
        [Authorize]
        [HttpPut("status")]
        public async Task<IActionResult> UpdateReservationStatus([FromBody] UpdateReservationStatusDto dto)
        {
            var currentUser = await GetCurrentUserAsync();

            if (currentUser == null)
            {
                return Unauthorized(new { message = "請先登入" });
            }

            var reservation = await _context.HouseViewings
                .FirstOrDefaultAsync(v => v.Id == dto.ReservationId);

            if (reservation == null)
            {
                return NotFound(new { message = "找不到此預約單" });
            }

            if (reservation.LessorId != currentUser.Id)
            {
                return Forbid();
            }

            if (dto.Status == "confirmed")
            {
                reservation.Status = 1;
                reservation.RejectReason = null;
            }
            else if (dto.Status == "rejected")
            {
                reservation.Status = 2;
                reservation.RejectReason = dto.RejectReason;
            }

            reservation.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "預約狀態已更新",
                reservationId = reservation.Id,
                status = dto.Status
            });
        }

        // 提議改期
        [Authorize]
        [HttpPut("reschedule")]
        public async Task<IActionResult> ProposeReschedule([FromBody] ProposeRescheduleDto dto)
        {
            var currentUser = await GetCurrentUserAsync();

            if (currentUser == null)
            {
                return Unauthorized(new { message = "請先登入" });
            }

            var reservation = await _context.HouseViewings
                .FirstOrDefaultAsync(v => v.Id == dto.ReservationId);

            if (reservation == null)
            {
                return NotFound(new { message = "找不到此預約單" });
            }

            if (reservation.LessorId != currentUser.Id)
            {
                return Forbid();
            }

            if (dto.ProposedEndTime.HasValue && dto.ProposedEndTime <= dto.ProposedStartTime)
            {
                return BadRequest(new { message = "改期結束時間必須晚於開始時間" });
            }

            reservation.Status = 3;
            reservation.RescheduleProposedTime = dto.ProposedStartTime;
            reservation.RescheduleEndTime = dto.ProposedEndTime;
            reservation.RescheduleMessage = dto.Message;
            reservation.RescheduleCount += 1;
            reservation.LastRescheduleAt = DateTime.Now;
            reservation.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "已提出改期",
                reservationId = reservation.Id,
                status = "rescheduled"
            });
        }

        private static List<string> ParseLesseeProfileTags(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<string>();
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var result = new List<string>();

                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                {
                    return result;
                }

                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    if (item.TryGetProperty("label", out var labelProp))
                    {
                        var label = labelProp.GetString();

                        if (!string.IsNullOrWhiteSpace(label))
                        {
                            result.Add(label);
                        }
                    }
                }

                return result;
            }
            catch
            {
                return new List<string>();
            }
        }

        [Authorize]
        [HttpPut("reselect-time")]
        public async Task<IActionResult> ReselectViewingTime([FromBody] ReselectViewingTimeDto dto)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();

                if (currentUser == null)
                {
                    return Unauthorized(new { message = "請先登入" });
                }

                var reservation = await _context.HouseViewings
                    .FirstOrDefaultAsync(v => v.Id == dto.ReservationId);

                if (reservation == null)
                {
                    return NotFound(new { message = "找不到此預約單" });
                }

                if (reservation.LesseeId != currentUser.Id)
                {
                    return Forbid();
                }

                if ((reservation.Status ?? 0) != 3)
                {
                    return BadRequest(new { message = "只有房東提議改期中的預約可以重新選擇時段" });
                }

                if (dto.ViewingSlotId.HasValue)
                {
                    var selectedSlot = await _context.HouseViewingAvailableSlots
                        .FirstOrDefaultAsync(s =>
                            s.Id == dto.ViewingSlotId.Value &&
                            s.HouseId == reservation.HouseId &&
                            s.IsEnabled);

                    if (selectedSlot == null)
                    {
                        return BadRequest(new { message = "選擇的看房時段不存在或已停用" });
                    }
                }

                reservation.ViewingSlotId = dto.ViewingSlotId;
                reservation.ViewingTime = dto.ViewingTime;

                reservation.ExpectedMoveIn = dto.ExpectedMoveIn ?? reservation.ExpectedMoveIn;
                reservation.ExpectedMoveInText = dto.ExpectedMoveInText;

                reservation.PreferredTimeSlotsJson = JsonSerializer.Serialize(dto.PreferredTimeSlots ?? new List<string>());
                reservation.LesseeProfileTagsJson = JsonSerializer.Serialize(dto.LesseeProfileTags ?? new List<LesseeProfileTagDto>());

                reservation.Message = dto.Message;
                reservation.MatchScore = dto.MatchScore ?? reservation.MatchScore;

                // 關鍵：承租人重新選擇後，回到出租人待審核
                reservation.Status = 0;
                reservation.UpdatedAt = DateTime.Now;

                reservation.ApplicationFlowType = "reselect_time";

                reservation.AttemptNo = reservation.AttemptNo <= 0
                    ? 1
                    : reservation.AttemptNo;

                reservation.MaxAttemptCount = reservation.MaxAttemptCount <= 0
                    ? 3
                    : reservation.MaxAttemptCount;

                // 清除上一輪房東提議改期內容，避免前端仍顯示舊改期訊息
                reservation.RescheduleProposedTime = null;
                reservation.RescheduleEndTime = null;
                reservation.RescheduleMessage = null;

                reservation.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "已重新送出看房時段，等待出租人審核",
                    reservationId = reservation.Id,
                    status = "pending"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "重新選擇時段失敗",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [Authorize]
        [HttpPut("accept-reschedule/{reservationId:int}")]
        public async Task<IActionResult> AcceptReschedule(int reservationId)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();

                if (currentUser == null)
                {
                    return Unauthorized(new { message = "請先登入" });
                }

                var reservation = await _context.HouseViewings
                    .FirstOrDefaultAsync(v => v.Id == reservationId);

                if (reservation == null)
                {
                    return NotFound(new { message = "找不到此預約單" });
                }

                // 只能承租人本人接受改期
                if (reservation.LesseeId != currentUser.Id)
                {
                    return Forbid();
                }

                // 只有房東已提出改期中的預約可以接受
                if ((reservation.Status ?? 0) != 3)
                {
                    return BadRequest(new { message = "此預約目前不是待回覆改期狀態" });
                }

                if (!reservation.RescheduleProposedTime.HasValue)
                {
                    return BadRequest(new { message = "此預約沒有房東提議的改期時間" });
                }

                // 接受房東改期：正式確認預約
                reservation.Status = 1;

                // 將最終看房時間改成房東提議的時間
                reservation.ViewingTime = reservation.RescheduleProposedTime.Value;

                // 保留紀錄用：代表這筆是接受改期後確認
                reservation.ApplicationFlowType = "reschedule_accepted";

                reservation.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "已接受改期，預約已確認",
                    reservationId = reservation.Id,
                    status = "confirmed"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "接受改期失敗",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [Authorize]
        [HttpPut("reapply")]
        public async Task<IActionResult> ReapplyViewingOrder([FromBody] ReapplyViewingOrderDto dto)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();

                if (currentUser == null)
                {
                    return Unauthorized(new { message = "請先登入" });
                }

                var reservation = await _context.HouseViewings
                    .FirstOrDefaultAsync(v => v.Id == dto.ReservationId);

                if (reservation == null)
                {
                    return NotFound(new { message = "找不到此預約單" });
                }

                if (reservation.LesseeId != currentUser.Id)
                {
                    return Forbid();
                }

                if ((reservation.Status ?? 0) != 2)
                //if (reservation.Status != 2)
                {
                    return BadRequest(new { message = "只有已婉拒的預約可以重新申請" });
                }

                var currentAttemptNo = reservation.AttemptNo <= 0 ? 1 : reservation.AttemptNo;
                var maxAttemptCount = reservation.MaxAttemptCount <= 0 ? 3 : reservation.MaxAttemptCount;

                if (currentAttemptNo >= maxAttemptCount)
                {
                    return BadRequest(new
                    {
                        message = $"此預約已達重新申請上限，最多可申請 {maxAttemptCount} 次"
                    });
                }

                if (dto.ViewingSlotId.HasValue)
                {
                    var selectedSlot = await _context.HouseViewingAvailableSlots
                        .FirstOrDefaultAsync(s =>
                            s.Id == dto.ViewingSlotId.Value &&
                            s.HouseId == reservation.HouseId &&
                            s.IsEnabled);

                    if (selectedSlot == null)
                    {
                        return BadRequest(new { message = "選擇的看房時段不存在或已停用" });
                    }
                }

                reservation.ViewingSlotId = dto.ViewingSlotId;
                reservation.ViewingTime = dto.ViewingTime;

                reservation.ExpectedMoveIn = dto.ExpectedMoveIn ?? reservation.ExpectedMoveIn;
                reservation.ExpectedMoveInText = dto.ExpectedMoveInText;

                reservation.PreferredTimeSlotsJson = JsonSerializer.Serialize(dto.PreferredTimeSlots ?? new List<string>());
                reservation.LesseeProfileTagsJson = JsonSerializer.Serialize(dto.LesseeProfileTags ?? new List<LesseeProfileTagDto>());

                reservation.Message = dto.Message;
                reservation.MatchScore = dto.MatchScore ?? reservation.MatchScore;

                // 關鍵：重新申請後回到出租人待審核
                reservation.Status = 0;
                reservation.ApplicationFlowType = "reapply";
                reservation.AttemptNo = currentAttemptNo + 1;
                reservation.MaxAttemptCount = maxAttemptCount;

                // 清除舊的婉拒原因與改期資訊，避免前端誤顯示
                reservation.RejectReason = null;
                reservation.RescheduleProposedTime = null;
                reservation.RescheduleEndTime = null;
                reservation.RescheduleMessage = null;

                reservation.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "已重新送出申請，等待出租人審核",
                    reservationId = reservation.Id,
                    status = "pending",
                    attemptNo = reservation.AttemptNo,
                    maxAttemptCount = reservation.MaxAttemptCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "重新申請失敗",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [Authorize]
        [HttpPut("confirm-match")]
        public async Task<IActionResult> ConfirmMatch([FromBody] ConfirmMatchDto dto)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();

                if (currentUser == null)
                {
                    return Unauthorized(new { message = "請先登入" });
                }

                var reservation = await _context.HouseViewings
                    .FirstOrDefaultAsync(v => v.Id == dto.ReservationId);

                if (reservation == null)
                {
                    return NotFound(new { message = "找不到此預約單" });
                }

                if (reservation.LessorId != currentUser.Id)
                {
                    return Forbid();
                }

                if ((reservation.Status ?? 0) != 1)
                {
                    return BadRequest(new { message = "只有已確認的預約可以確認媒合" });
                }

                var house = await _context.Rent_Houses
                    .FirstOrDefaultAsync(h => h.Id == reservation.HouseId);

                if (house == null)
                {
                    return NotFound(new { message = "找不到此房源" });
                }

                var now = DateTime.Now;

                // 1. 將本筆預約標記為媒合成功
                reservation.Status = 4;
                reservation.MatchedAt = now;
                reservation.MatchedByUserId = currentUser.Id;
                reservation.MatchNote = dto.MatchNote;
                reservation.ApplicationFlowType = "matched";
                reservation.UpdatedAt = now;

                // 2. 同步標記房源已媒合 / 下架
                if (dto.MarkHouseAsMatched)
                {
                    house.RentalStatus = "matched";
                    house.MatchedViewingOrderId = reservation.Id;
                    house.MatchedAt = now;
                    house.IsVisible = false;
                }

                // 3. 關閉同房源其他尚未結束的預約
                if (dto.CloseOtherReservations)
                {
                    var otherReservations = await _context.HouseViewings
                        .Where(v =>
                            v.HouseId == reservation.HouseId &&
                            v.Id != reservation.Id &&
                            (
                                v.Status == 0 ||
                                v.Status == 1 ||
                                v.Status == 3
                            ))
                        .ToListAsync();

                    foreach (var other in otherReservations)
                    {
                        other.Status = 5;
                        other.ClosedReason = "此房源已完成媒合，申請已關閉";
                        other.UpdatedAt = now;
                        other.ApplicationFlowType = "closed_by_match";
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "已確認媒合成功",
                    reservationId = reservation.Id,
                    status = "matched",
                    houseId = reservation.HouseId,
                    houseStatus = house.RentalStatus
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "確認媒合失敗",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
        //[Authorize]
        [HttpGet("getallapplications")]
        public async Task<IActionResult> GetAllApplications() {
            var res = await _context.HouseViewings.ToListAsync();
            if(res == null) {
                return NotFound();
            }
            return Ok(res);
        }
    }
}