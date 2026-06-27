using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RentApi.Data;
using RentApi.Models;
using RentApi.Models.DTO;
using System.Security.Claims;

namespace RentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductBookingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductBookingController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 承租人送出工具 / 技能預約
        // POST api/ProductBooking/apply
        // ============================================================
        [Authorize]
        [HttpPost("apply")]
        public async Task<IActionResult> CreateApplication([FromBody] CreateProductBookingDto dto)
        {
            var currentUser = await GetCurrentUserAsync();

            if (currentUser == null)
            {
                return Unauthorized(new { message = "請先登入後再送出預約" });
            }

            var product = await _context.Rent_Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product == null)
            {
                return BadRequest(new { message = $"找不到 ID 為 {dto.ProductId} 的工具 / 技能資料" });
            }

            if (product.Status != 1)
            {
                return BadRequest(new { message = "此工具 / 技能尚未上架或已下架，無法預約" });
            }

            var providerUser = await _context.User
                .FirstOrDefaultAsync(u => u.AccountId == product.AccountId);

            if (providerUser == null)
            {
                return BadRequest(new { message = $"找不到此工具 / 技能的提供者資料，AccountId = {product.AccountId}" });
            }

            if (providerUser.Id == currentUser.Id)
            {
                return BadRequest(new { message = "不能預約自己發布的工具 / 技能" });
            }

            var bookingKind = ResolveBookingKind(product.Category, dto.BookingType);

            if (bookingKind == "skill")
            {
                return await CreateSkillBooking(dto, product, currentUser, providerUser);
            }

            return await CreateToolBooking(dto, product, currentUser, providerUser);
        }

        // ============================================================
        // 出租人 / 提供者取得自己的工具 / 技能預約審核列表
        // GET api/ProductBooking/my-approvals
        // ============================================================
        [Authorize]
        [HttpGet("my-approvals")]
        public async Task<IActionResult> GetMyApprovals()
        {
            var currentUser = await GetCurrentUserAsync();

            if (currentUser == null)
            {
                return Unauthorized(new { message = "請先登入" });
            }

            var toolRaw = await (
                from b in _context.ToolBookingOrders.AsNoTracking()
                join p in _context.Rent_Products.AsNoTracking()
                    on b.ToolId equals p.Id
                join borrower in _context.User.AsNoTracking()
                    on b.BorrowerId equals borrower.Id into borrowerJoin
                from borrower in borrowerJoin.DefaultIfEmpty()
                where b.LenderId == currentUser.Id
                select new
                {
                    Booking = b,
                    Product = p,
                    Applicant = borrower
                }
            ).ToListAsync();

            var skillRaw = await (
                from b in _context.SkillBookingOrders.AsNoTracking()
                join p in _context.Rent_Products.AsNoTracking()
                    on b.SkillId equals p.Id
                join learner in _context.User.AsNoTracking()
                    on b.LearnerId equals learner.Id into learnerJoin
                from learner in learnerJoin.DefaultIfEmpty()
                where b.MentorId == currentUser.Id
                select new
                {
                    Booking = b,
                    Product = p,
                    Applicant = learner
                }
            ).ToListAsync();

            var productIds = toolRaw
                .Select(x => x.Product.Id)
                .Concat(skillRaw.Select(x => x.Product.Id))
                .Distinct()
                .ToList();

            var imageMap = await _context.Product_Image
                .AsNoTracking()
                .Where(img => productIds.Contains(img.ProductId))
                .OrderByDescending(img => img.IsCover)
                .ThenBy(img => img.Id)
                .GroupBy(img => img.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Url = g.Select(x => x.Url).FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.ProductId, x => x.Url ?? "");

            var tools = toolRaw
                .Select(x => ToolToResponseDto(
                    x.Booking,
                    x.Product,
                    x.Applicant,
                    currentUser,
                    imageMap.ContainsKey(x.Product.Id) ? imageMap[x.Product.Id] : ""
                ))
                .ToList();

            var skills = skillRaw
                .Select(x => SkillToResponseDto(
                    x.Booking,
                    x.Product,
                    x.Applicant,
                    currentUser,
                    imageMap.ContainsKey(x.Product.Id) ? imageMap[x.Product.Id] : ""
                ))
                .ToList();

            var result = tools
                .Concat(skills)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return Ok(result);
        }

        // ============================================================
        // 承租人取得自己送出的工具 / 技能預約紀錄
        // GET api/ProductBooking/my-applications
        // ============================================================
        [Authorize]
        [HttpGet("my-applications")]
        public async Task<IActionResult> GetMyApplications()
        {
            var currentUser = await GetCurrentUserAsync();

            if (currentUser == null)
            {
                return Unauthorized(new { message = "請先登入" });
            }

            var toolRaw = await (
                from b in _context.ToolBookingOrders.AsNoTracking()
                join p in _context.Rent_Products.AsNoTracking()
                    on b.ToolId equals p.Id
                join provider in _context.User.AsNoTracking()
                    on b.LenderId equals provider.Id into providerJoin
                from provider in providerJoin.DefaultIfEmpty()
                where b.BorrowerId == currentUser.Id
                select new
                {
                    Booking = b,
                    Product = p,
                    Provider = provider
                }
            ).ToListAsync();

            var skillRaw = await (
                from b in _context.SkillBookingOrders.AsNoTracking()
                join p in _context.Rent_Products.AsNoTracking()
                    on b.SkillId equals p.Id
                join provider in _context.User.AsNoTracking()
                    on b.MentorId equals provider.Id into providerJoin
                from provider in providerJoin.DefaultIfEmpty()
                where b.LearnerId == currentUser.Id
                select new
                {
                    Booking = b,
                    Product = p,
                    Provider = provider
                }
            ).ToListAsync();

            var productIds = toolRaw
                .Select(x => x.Product.Id)
                .Concat(skillRaw.Select(x => x.Product.Id))
                .Distinct()
                .ToList();

            var imageMap = await _context.Product_Image
                .AsNoTracking()
                .Where(img => productIds.Contains(img.ProductId))
                .OrderByDescending(img => img.IsCover)
                .ThenBy(img => img.Id)
                .GroupBy(img => img.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Url = g.Select(x => x.Url).FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.ProductId, x => x.Url ?? "");

            var tools = toolRaw
                .Select(x => ToolToResponseDto(
                    x.Booking,
                    x.Product,
                    currentUser,
                    x.Provider,
                    imageMap.ContainsKey(x.Product.Id) ? imageMap[x.Product.Id] : ""
                ))
                .ToList();

            var skills = skillRaw
                .Select(x => SkillToResponseDto(
                    x.Booking,
                    x.Product,
                    currentUser,
                    x.Provider,
                    imageMap.ContainsKey(x.Product.Id) ? imageMap[x.Product.Id] : ""
                ));

            var result = tools
                .Concat(skills)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return Ok(result);
        }

        // ============================================================
        // 出租人 / 提供者更新預約狀態
        // PATCH api/ProductBooking/UpdateStatus
        // ============================================================
        [Authorize]
        [HttpPatch("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateProductBookingStatusDto dto)
        {
            var currentUser = await GetCurrentUserAsync();

            if (currentUser == null)
            {
                return Unauthorized(new { message = "請先登入" });
            }

            var kind = NormalizeBookingKind(dto.BookingKind);
            var newStatus = StatusTextToInt(dto.Status);

            if (kind == "skill")
            {
                var booking = await _context.SkillBookingOrders
                    .FirstOrDefaultAsync(b => b.Id == dto.ReservationId);

                if (booking == null)
                {
                    return NotFound(new { message = "找不到該筆技能預約單" });
                }

                if (booking.MentorId != currentUser.Id)
                {
                    return Forbid();
                }

                booking.Status = newStatus;
                booking.CurrentStatus = dto.Status;
                booking.RejectReason = dto.RejectReason;
                booking.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { message = "技能預約狀態已更新" });
            }
            else
            {
                var booking = await _context.ToolBookingOrders
                    .FirstOrDefaultAsync(b => b.Id == dto.ReservationId);

                if (booking == null)
                {
                    return NotFound(new { message = "找不到該筆工具預約單" });
                }

                if (booking.LenderId != currentUser.Id)
                {
                    return Forbid();
                }

                booking.Status = newStatus;
                booking.RejectReason = dto.RejectReason;
                booking.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { message = "工具預約狀態已更新" });
            }
        }

        // ============================================================
        // 建立工具預約
        // ============================================================
        private async Task<IActionResult> CreateToolBooking(
            CreateProductBookingDto dto,
            RentProduct product,
            User borrower,
            User lender
        )
        {
            if (!dto.StartTime.HasValue || !dto.EndTime.HasValue)
            {
                return BadRequest(new { message = "請選擇工具借用開始與結束日期" });
            }

            var startDate = dto.StartTime.Value.Date;
            var endDate = dto.EndTime.Value.Date;

            if (endDate < startDate)
            {
                return BadRequest(new { message = "結束日期不可早於開始日期" });
            }

            var totalDays = Math.Max(1, (endDate - startDate).Days);

            var price = product.Price ?? 0;
            var deposit = product.Deposit ?? 0;

            var totalFee = price * totalDays;

            var uniqueId = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
            var orderNo = $"T-{DateTime.Now:yyyyMMdd}-{uniqueId}";

            var booking = new ToolBookingOrder
            {
                ReservationNo = orderNo,
                ToolId = product.Id,
                BorrowerId = borrower.Id,
                LenderId = lender.Id,
                StartDate = startDate,
                EndDate = endDate,
                ActualReturnDate = null,
                TotalFee = totalFee,
                DepositFee = deposit,
                TotalDays = totalDays,
                DeliveryMethod = DeliveryMethodToInt(dto.Method),
                ShippingAddress = dto.ShippingAddress,
                TrackingNumber = null,
                Purpose = dto.Message ?? string.Empty,
                Status = 0,
                RejectReason = null,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.ToolBookingOrders.Add(booking);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "工具借用申請已送出",
                orderNumber = orderNo
            });
        }

        // ============================================================
        // 建立技能預約
        // ============================================================
        private async Task<IActionResult> CreateSkillBooking(
            CreateProductBookingDto dto,
            RentProduct product,
            User learner,
            User mentor
        )
        {
            if (!dto.StartTime.HasValue)
            {
                return BadRequest(new { message = "請選擇技能預約日期" });
            }

            var start = dto.StartTime.Value;
            var end = dto.EndTime ?? start.AddHours(1);

            if (end <= start)
            {
                end = start.AddHours(1);
            }

            var totalHours = Math.Max(1m, Convert.ToDecimal((end - start).TotalHours));

            var price = product.Price ?? 0;
            var totalFee = Convert.ToInt32(price * totalHours);

            var uniqueId = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
            var bookingNo = $"S-{DateTime.Now:yyyyMMdd}-{uniqueId}";

            var booking = new SkillBookingOrder
            {
                BookingNo = bookingNo,
                SkillId = product.Id,
                SessionId = null,
                LearnerId = learner.Id,
                MentorId = mentor.Id,
                BookingType = 1,
                BookingDate = DateOnly.FromDateTime(start),
                StartTime = TimeOnly.FromDateTime(start),
                EndTime = TimeOnly.FromDateTime(end),
                TotalFee = totalFee,
                TotalHours = totalHours,
                MeetingMethod = MeetingMethodToInt(dto.Method),
                MeetingUrl = dto.MeetingUrl,
                MeetingLocation = dto.MeetingLocation,
                CurrentStatus = "pending",
                Status = 0,
                RejectReason = null,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.SkillBookingOrders.Add(booking);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "技能預約申請已送出",
                orderNumber = bookingNo
            });
        }

        // ============================================================
        // Response Mapping
        // ============================================================
        private static ProductBookingResponseDto ToolToResponseDto(
            ToolBookingOrder booking,
            RentProduct product,
            User? applicant,
            User? provider,
            string coverUrl
        )
        {
            return new ProductBookingResponseDto
            {
                Id = booking.Id.ToString(),
                OrderNumber = booking.ReservationNo,
                Status = StatusIntToText(booking.Status),
                Type = "tool",
                ProductId = product.Id,
                ItemName = product.Name ?? "未知工具",
                CoverUrl = coverUrl,
                PriceInfo = $"NT$ {booking.TotalFee:N0}，押金 NT$ {booking.DepositFee:N0}",
                BookingPeriod = $"{booking.StartDate:yyyy/MM/dd} - {booking.EndDate:yyyy/MM/dd}（共 {booking.TotalDays} 天）",
                Method = DeliveryMethodToText(booking.DeliveryMethod),
                ExtraNote = string.Empty,
                Message = string.IsNullOrWhiteSpace(booking.Purpose) ? "無留言" : booking.Purpose,
                MatchScore = 0,
                CreatedAt = booking.CreatedAt,
                Applicant = new ProductBookingApplicantDto
                {
                    Name = applicant?.RealName ?? "未知申請人",
                    Avatar = "images/mr_chen.jpg",
                    Profiles = new List<string>(),
                    Phone = applicant?.Phone ?? "未填寫",
                    LineId = applicant?.LineId ?? "未填寫"
                },
                Provider = new ProductBookingProviderDto
                {
                    Name = provider?.RealName ?? "未知提供者",
                    Avatar = "images/mr_chen.jpg",
                    Phone = provider?.Phone ?? "未填寫",
                    LineId = provider?.LineId ?? "未填寫"
                }
            };
        }

        private static ProductBookingResponseDto SkillToResponseDto(
            SkillBookingOrder booking,
            RentProduct product,
            User? applicant,
            User? provider,
            string coverUrl
        )
        {
            var bookingDate = booking.BookingDate.HasValue
                ? booking.BookingDate.Value.ToString("yyyy/MM/dd")
                : "尚未選擇日期";

            var timeText = booking.StartTime.HasValue
                ? $"{booking.StartTime.Value:HH\\:mm} - {(booking.EndTime.HasValue ? booking.EndTime.Value.ToString("HH\\:mm") : "")}"
                : "尚未選擇時間";

            return new ProductBookingResponseDto
            {
                Id = booking.Id.ToString(),
                OrderNumber = booking.BookingNo,
                Status = StatusIntToText(booking.Status),
                Type = "skill",
                ProductId = product.Id,
                ItemName = product.Name ?? "未知技能",
                CoverUrl = coverUrl,
                PriceInfo = $"NT$ {booking.TotalFee:N0} / {booking.TotalHours} 小時",
                BookingPeriod = $"{bookingDate} {timeText}",
                Method = MeetingMethodToText(booking.MeetingMethod),
                ExtraNote = booking.CurrentStatus ?? string.Empty,
                Message = string.IsNullOrWhiteSpace(product.Description) ? "無留言" : product.Description,
                MatchScore = 0,
                CreatedAt = booking.CreatedAt,
                Applicant = new ProductBookingApplicantDto
                {
                    Name = applicant?.RealName ?? "未知申請人",
                    Avatar = "images/mr_chen.jpg",
                    Profiles = new List<string>(),
                    Phone = applicant?.Phone ?? "未填寫",
                    LineId = applicant?.LineId ?? "未填寫"
                },
                Provider = new ProductBookingProviderDto
                {
                    Name = provider?.RealName ?? "未知提供者",
                    Avatar = "images/mr_chen.jpg",
                    Phone = provider?.Phone ?? "未填寫",
                    LineId = provider?.LineId ?? "未填寫"
                }
            };
        }

        // ============================================================
        // Helper
        // ============================================================
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

        private static string ResolveBookingKind(string? category, string? requestedType)
        {
            if (!string.IsNullOrWhiteSpace(requestedType))
            {
                return NormalizeBookingKind(requestedType);
            }

            return category == "專業諮詢" || category == "技能"
                ? "skill"
                : "tool";
        }

        private static string NormalizeBookingKind(string? value)
        {
            return value == "skill" ? "skill" : "tool";
        }

        private static int StatusTextToInt(string status)
        {
            return status switch
            {
                "confirmed" => 1,
                "rejected" => 2,
                "rescheduled" => 3,
                "closed" => 4,
                _ => 0
            };
        }

        private static string StatusIntToText(int status)
        {
            return status switch
            {
                1 => "confirmed",
                2 => "rejected",
                3 => "rescheduled",
                4 => "closed",
                _ => "pending"
            };
        }

        private static int DeliveryMethodToInt(string? method)
        {
            if (string.IsNullOrWhiteSpace(method)) return 1;

            return method.Contains("物流") || method.Contains("寄送")
                ? 2
                : 1;
        }

        private static string DeliveryMethodToText(int method)
        {
            return method switch
            {
                2 => "物流寄送",
                _ => "面交自取"
            };
        }

        private static int MeetingMethodToInt(string? method)
        {
            if (string.IsNullOrWhiteSpace(method)) return 1;

            return method.Contains("實體")
                ? 2
                : 1;
        }

        private static string MeetingMethodToText(int method)
        {
            return method switch
            {
                2 => "實體面授",
                _ => "線上視訊"
            };
        }
    }
}
