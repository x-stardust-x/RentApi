
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using RentApi.Data;
using RentApi.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentHouseController : Controller
    {
        private readonly AppDbContext _context;

        public RentHouseController(AppDbContext context)
        {
            _context = context;
        }

        // 1. 新增房屋 (已同步最新欄位)
        [HttpPost]
        public IActionResult CreateHouse([FromBody] CreateHouseDto request)
        {
            if (request == null) return BadRequest("房屋資料不能為空");

            var newHouse = new Rent_House
            {
                AccountId = request.AccountId,
                DistrictId = request.DistrictId,
                Name = request.Name,
                Address = request.Address,
                Description = request.Description,
                RentPrice = request.RentPrice,
                IncludeUtilities = request.IncludeUtilities,
                IncludeWifi = request.IncludeWifi,
                IncludeManagementFee = request.IncludeManagementFee,
                AreaSize = request.AreaSize,
                LeaseTerm = request.LeaseTerm,
                FloorInfo = request.FloorInfo,
                HouseType = request.HouseType,
                ViewCount = request.ViewCount ?? 0,
                Status = request.Status
            };

            _context.Rent_Houses.Add(newHouse);
            _context.SaveChanges();

            var newRule = new HouseRules
            {
                HouseId = newHouse.Id,
                SleepTime = request.SleepTime,
                WakeTime = request.WakeTime,
                CleanLevel = request.CleanLevel,
                NoiseTolerance = request.NoiseTolerance,
                Pet = request.Pet,
                Smoke = request.Smoke,
                LivingWithLessor = request.LivingWithLessor,
                AdvancedRules = request.AdvancedRules ?? string.Empty
            };

            _context.HouseRules.Add(newRule);
            _context.SaveChanges();

            return Ok(newHouse);
        }

        // 2. 取得房屋列表 
        [HttpGet]
        public async Task<IActionResult> GetAllHouses()
        {
            try
            {
                // 開發期先拉長 timeout，避免 SQL Server 偶發慢查直接中斷
                _context.Database.SetCommandTimeout(60);

                var houses = await (
                    from h in _context.Rent_Houses.AsNoTracking()

                    join loc in _context.Location_Districts.AsNoTracking()
                        on h.DistrictId equals loc.DistrictId into locations
                    from loc in locations.DefaultIfEmpty()

                    join acc in _context.Account.AsNoTracking()
                        on h.AccountId equals acc.Id into accounts
                    from acc in accounts.DefaultIfEmpty()

                    join rule in _context.HouseRules.AsNoTracking()
                        on h.Id equals rule.HouseId into rules
                    from rule in rules.DefaultIfEmpty()

                    select new
                    {
                        h.Id,
                        h.AccountId,
                        h.DistrictId,
                        h.Name,
                        h.Address,
                        h.RentPrice,
                        h.HouseType,
                        h.AreaSize,
                        h.Status,
                        h.Description,
                        h.FloorInfo,
                        h.IncludeUtilities,
                        h.IncludeWifi,
                        h.IncludeManagementFee,

                        CityName = loc != null ? loc.CityName : "",
                        DistrictName = loc != null ? loc.DistrictName : "",

                        UserName = acc != null ? acc.Username : "神祕房東",
                        UserAvatar = "",

                        SleepTime = rule != null ? rule.SleepTime : null,
                        WakeTime = rule != null ? rule.WakeTime : null,
                        CleanLevel = rule != null ? rule.CleanLevel : 3,
                        NoiseTolerance = rule != null ? rule.NoiseTolerance : 3,
                        Pet = rule != null ? rule.Pet : false,
                        Smoke = rule != null ? rule.Smoke : false,
                        LivingWithLessor = rule != null ? rule.LivingWithLessor : false,
                        AdvancedRules = rule != null ? rule.AdvancedRules : ""
                    }
                )
                .OrderByDescending(h => h.Id)
                .ToListAsync();

                var houseIds = houses.Select(h => h.Id).ToList();

                var images = await _context.House_Images
                    .AsNoTracking()
                    .Where(img => houseIds.Contains(img.HouseId))
                    .Select(img => new
                    {
                        img.Id,
                        img.HouseId,
                        img.Url,
                        img.IsCover
                    })
                    .ToListAsync();

                var result = houses.Select(h => new
                {
                    h.Id,
                    h.AccountId,
                    h.DistrictId,
                    h.Name,
                    h.Address,
                    h.RentPrice,
                    h.HouseType,
                    h.AreaSize,
                    h.Status,
                    Description = h.Description,

                    h.CityName,
                    h.DistrictName,

                    h.UserName,
                    h.UserAvatar,

                    SleepTime = h.SleepTime != null ? h.SleepTime.Value.ToString("HH:mm") : null,
                    WakeTime = h.WakeTime != null ? h.WakeTime.Value.ToString("HH:mm") : null,

                    h.CleanLevel,
                    h.NoiseTolerance,
                    h.Pet,
                    h.Smoke,
                    h.LivingWithLessor,
                    h.AdvancedRules,

                    h.FloorInfo,
                    h.IncludeUtilities,
                    h.IncludeWifi,
                    h.IncludeManagementFee,

                    // 保留你原本 typo 欄位，避免舊前端壞掉
                    IncludeManagememtFee = h.IncludeManagementFee,

                    CoverUrl = images
                        .Where(img => img.HouseId == h.Id && img.IsCover)
                        .Select(img => img.Url)
                        .FirstOrDefault()
                        ?? images
                            .Where(img => img.HouseId == h.Id)
                            .Select(img => img.Url)
                            .FirstOrDefault(),

                    Images = images
                        .Where(img => img.HouseId == h.Id)
                        .Select(img => new
                        {
                            img.Id,
                            img.Url,
                            img.IsCover
                        })
                        .ToList()
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "取得房屋列表失敗",
                    Details = ex.Message,
                    Inner = ex.InnerException?.Message
                });
            }
        }

        // 🌟 取得「我的房源」列表 (專屬房東)
        [HttpGet("GetMyHouses/{accountId:int}")]
        public IActionResult GetMyHouses(int accountId)
        {
            try
            {
                var myHouses = (
                    from h in _context.Rent_Houses.AsNoTracking()

                    join loc in _context.Location_Districts.AsNoTracking()
                        on h.DistrictId equals loc.DistrictId into locGroup
                    from loc in locGroup.DefaultIfEmpty()

                    join rule in _context.HouseRules.AsNoTracking()
                        on h.Id equals rule.HouseId into ruleGroup
                    from rule in ruleGroup.DefaultIfEmpty()

                    where h.AccountId == accountId

                    select new
                    {
                        h.Id,
                        h.AccountId,
                        h.DistrictId,
                        h.Name,
                        h.Address,
                        h.Description,
                        h.RentPrice,
                        h.Status,
                        h.AreaSize,
                        h.LeaseTerm,
                        h.FloorInfo,
                        h.HouseType,
                        h.IncludeUtilities,
                        h.IncludeWifi,
                        h.IncludeManagementFee,

                        CityName = loc != null ? loc.CityName : "",
                        DistrictName = loc != null ? loc.DistrictName : "未知區域",
                        ZipCode = loc != null ? loc.ZipCode.ToString() : "",

                        SleepTime = rule != null ? rule.SleepTime : null,
                        WakeTime = rule != null ? rule.WakeTime : null,
                        CleanLevel = rule != null ? rule.CleanLevel : 3,
                        NoiseTolerance = rule != null ? rule.NoiseTolerance : 3,
                        Pet = rule != null ? rule.Pet : false,
                        Smoke = rule != null ? rule.Smoke : false,
                        LivingWithLessor = rule != null ? rule.LivingWithLessor : false,
                        AdvancedRules = rule != null ? rule.AdvancedRules : ""
                    }
                ).ToList();

                var houseIds = myHouses.Select(h => h.Id).ToList();

                var images = _context.House_Images
                    .AsNoTracking()
                    .Where(img => houseIds.Contains(img.HouseId))
                    .Select(img => new
                    {
                        img.Id,
                        img.HouseId,
                        img.Url,
                        img.IsCover
                    })
                    .ToList();

                var result = myHouses.Select(h => new
                {
                    h.Id,
                    h.AccountId,
                    h.DistrictId,
                    h.Name,
                    h.Address,
                    h.Description,
                    h.RentPrice,
                    h.Status,
                    h.AreaSize,
                    h.LeaseTerm,
                    h.FloorInfo,
                    h.HouseType,
                    h.IncludeUtilities,
                    h.IncludeWifi,
                    h.IncludeManagementFee,

                    h.CityName,
                    h.DistrictName,
                    h.ZipCode,

                    SleepTime = h.SleepTime != null ? h.SleepTime.Value.ToString("HH:mm") : null,
                    WakeTime = h.WakeTime != null ? h.WakeTime.Value.ToString("HH:mm") : null,
                    h.CleanLevel,
                    h.NoiseTolerance,
                    h.Pet,
                    h.Smoke,
                    h.AdvancedRules,

                    CoverUrl = images
                        .Where(img => img.HouseId == h.Id && img.IsCover)
                        .Select(img => img.Url)
                        .FirstOrDefault()
                        ?? images
                            .Where(img => img.HouseId == h.Id)
                            .Select(img => img.Url)
                            .FirstOrDefault(),

                    Images = images
                        .Where(img => img.HouseId == h.Id)
                        .Select(img => new
                        {
                            img.Id,
                            img.Url,
                            img.IsCover
                        })
                        .ToList()
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "系統發生錯誤",
                    Details = ex.Message
                });
            }
        }

        // 3. 取得單一房屋詳細資料
        [HttpGet("{id:int}")]
        public IActionResult GetHouseById(int id)
        {
            var house = _context.Rent_Houses
                .Include(h => h.HouseImages)
                .FirstOrDefault(h => h.Id == id);

            if (house == null)
            {
                return NotFound("找不到這間房子喔！");
            }

            var rule = _context.HouseRules
                .FirstOrDefault(r => r.HouseId == id);

            return Ok(new
            {
                house.Id,
                house.AccountId,
                house.DistrictId,
                house.Name,
                house.Address,
                house.Description,
                house.RentPrice,
                house.IncludeUtilities,
                house.IncludeWifi,
                house.IncludeManagementFee,
                house.AreaSize,
                house.LeaseTerm,
                house.FloorInfo,
                house.HouseType,
                house.Status,

                SleepTime = rule != null && rule.SleepTime.HasValue
                    ? rule.SleepTime.Value.ToString("HH:mm")
                    : "23:30",

                WakeTime = rule != null && rule.WakeTime.HasValue
                    ? rule.WakeTime.Value.ToString("HH:mm")
                    : "07:00",
                CleanLevel = rule != null ? rule.CleanLevel : 3,
                NoiseTolerance = rule != null ? rule.NoiseTolerance : 3,
                Pet = rule?.Pet ?? false,
                Smoke = rule?.Smoke ?? false,
                LivingWithLessor = rule != null ? rule.LivingWithLessor : false,
                AdvancedRules = rule != null ? rule.AdvancedRules : "",

                Images = house.HouseImages.Select(img => new
                {
                    img.Id,
                    img.Url,
                    img.IsCover
                }).ToList()
            });
        }

        //  4. 修改房屋資料 
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateHouse(int id, [FromBody] CreateHouseDto request)
        {
            try
            {
                var house = await _context.Rent_Houses
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (house == null)
                {
                    return NotFound("找不到要修改的房子！");
                }

                // 基本資料
                house.DistrictId = request.DistrictId;
                house.Name = request.Name;
                house.Address = request.Address;
                house.Description = request.Description;
                house.RentPrice = request.RentPrice;

                // 詳細設備與規格
                house.IncludeUtilities = request.IncludeUtilities;
                house.IncludeWifi = request.IncludeWifi;
                house.IncludeManagementFee = request.IncludeManagementFee;
                house.AreaSize = request.AreaSize;
                house.LeaseTerm = request.LeaseTerm;
                house.FloorInfo = request.FloorInfo;
                house.HouseType = request.HouseType;

                // 修改後重新送審
                house.Status = 0;

                var houseRule = await _context.HouseRules
                    .FirstOrDefaultAsync(r => r.HouseId == id);

                if (houseRule == null)
                {
                    houseRule = new HouseRules
                    {
                        HouseId = id
                    };

                    _context.HouseRules.Add(houseRule);
                }

                houseRule.SleepTime = request.SleepTime;
                houseRule.WakeTime = request.WakeTime;
                houseRule.CleanLevel = request.CleanLevel;
                houseRule.NoiseTolerance = request.NoiseTolerance;
                houseRule.Pet = request.Pet;
                houseRule.Smoke = request.Smoke;
                houseRule.LivingWithLessor = request.LivingWithLessor;
                houseRule.AdvancedRules = request.AdvancedRules ?? string.Empty;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "房屋資料更新成功！",
                    HouseData = house
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "房屋資料更新失敗",
                    Details = ex.Message,
                    Inner = ex.InnerException?.Message
                });
            }
        }

        // 核准上架
        [HttpPut("Approve/{id:int}")] 
        public IActionResult ApproveHouseStatus(int id)
        {
            var house = _context.Rent_Houses.Find(id);
            if (house == null) return NotFound("找不到這間房子喔！");

            house.Status = 1;
            _context.SaveChanges();

            return Ok(new { Message = "房屋核准成功！" });
        }

        // 強制下架 (將狀態改為 3)
        [HttpPut("TakeDown/{id:int}")] 
        public IActionResult TakeDownHouseStatus(int id)
        {
            var house = _context.Rent_Houses.Find(id);
            if (house == null) return NotFound("找不到這間房子喔！");

            house.Status = 3;
            _context.SaveChanges();

            return Ok(new { Message = "房屋已強制下架！" });
        }

        // 獲取所有行政區選單的 API
        [HttpGet("Districts")]
        public IActionResult GetDistricts()
        {
            var districts = _context.Location_Districts 
                .Select(d => new {
                    DistrictId = d.DistrictId,
                    CityName = d.CityName,
                    DistrictName = d.DistrictName
                })
                .ToList();

            return Ok(districts);
        }

        //  5. 刪除房屋
        [HttpDelete("{id:int}")]
        public IActionResult RejectHouse(int id)
        {
            var house = _context.Rent_Houses.Find(id);
            if (house == null) return NotFound("找不到該房屋申請！");

            var rules = _context.HouseRules.Where(r => r.HouseId == id).ToList();
            if (rules.Any())
            {
                _context.HouseRules.RemoveRange(rules);
            }

            var images = _context.House_Images.Where(i => i.HouseId == id).ToList();
            if (images.Any())
            {
                _context.House_Images.RemoveRange(images);
            }

            _context.Rent_Houses.Remove(house);
            _context.SaveChanges();

            return Ok(new { Message = "申請已成功退回，相關資料已刪除！" });
        }

       
        //  圖片操作區 (維持原樣，補上防護網)
       
        [HttpPost("Image/AddRecord")]
        public IActionResult AddImageRecord([FromBody] AddHouseImageDto request)
        {
            var newImage = new HouseImage
            {
                HouseId = request.HouseId,
                Url = request.Url,
                Description = request.Description,
                IsCover = request.IsCover
            };
            _context.House_Images.Add(newImage);
            _context.SaveChanges();
            return Ok(new { Message = "照片紀錄已成功寫入！", NewImageId = newImage.Id });
        }

        [HttpPut("Image/{imageId:int}/SetCover")] 
        public IActionResult SetCoverImage(int imageId)
        {
            var targetImage = _context.House_Images.Find(imageId);
            if (targetImage == null) return NotFound("找不到照片");

            var allImages = _context.House_Images.Where(img => img.HouseId == targetImage.HouseId).ToList();
            foreach (var img in allImages) img.IsCover = false;

            targetImage.IsCover = true;
            _context.SaveChanges();
            return Ok(new { Message = "🎉 首圖設定成功！" });
        }

        [HttpDelete("Image/{imageId:int}")]
        public IActionResult DeleteImage(int imageId)
        {
            var image = _context.House_Images.Find(imageId);
            if (image == null) return NotFound();

            var fileName = Path.GetFileName(image.Url);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", fileName);

            _context.House_Images.Remove(image);
            _context.SaveChanges();

            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            return Ok(new { Message = "刪除成功！" });
        }
    }
}

// ==========================================
// 📝 DTO 專區 (已同步最新資料表欄位)
// ==========================================
public class CreateHouseDto
    {
        public int? AccountId { get; set; }
        public int? DistrictId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public int RentPrice { get; set; }
        
       

        //  新增對應 bit 的 bool
        public bool IncludeUtilities { get; set; }
        public bool IncludeWifi { get; set; }
        public bool IncludeManagementFee { get; set; }

        //  新增規格
        public decimal? AreaSize { get; set; }
        public int? LeaseTerm { get; set; }
        public string FloorInfo { get; set; }
        public string HouseType { get; set; }
        public int? ViewCount { get; set; }
        public int Status { get; set; }


        // 🌟 新增：對應前端送過來的生活習慣資料
        public TimeOnly SleepTime { get; set; }
        public TimeOnly WakeTime { get; set; }
        public int CleanLevel { get; set; }
        public int NoiseTolerance { get; set; }
        public bool Pet { get; set; }
        public bool Smoke { get; set; }
        public bool LivingWithLessor { get; set; } = false;
        public string? Interests { get; set; }

        public string AdvancedRules { get; set; } = string.Empty;

        
    }


