using RentApi.Data;
using Microsoft.AspNetCore.Mvc;
using RentApi.Data;
using RentApi.Models;
using System.IO;
using System.Linq;
using CoLiving.models;

namespace CoLiving.Controllers
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
                DistrictId = request.DistrictId, // 🆕 新增
                Name = request.Name,
                Address = request.Address,
                Description = request.Description,
                RentPrice = request.RentPrice,
                // 🆕 以下為新增的詳細欄位
                IncludeUtilities = request.IncludeUtilities,
                IncludeWifi = request.IncludeWifi,
                IncludeManagementFee = request.IncludeManagementFee, // 照資料庫拼錯字
                AreaSize = request.AreaSize,
                LeaseTerm = request.LeaseTerm,
                FloorInfo = request.FloorInfo,
                HouseType = request.HouseType,
                ViewCount = request.ViewCount ?? 0, // 預設 0
                Status = request.Status
            };

            _context.Rent_Houses.Add(newHouse);
            _context.SaveChanges();

            return Ok(new
            {
                Message = " 房屋上架成功！",
                HouseId = newHouse.Id,
                HouseData = newHouse
            });
        }

        // 2. 取得房屋列表 
        [HttpGet]
        public IActionResult GetAllHouses()
        {
            var houses = _context.Rent_Houses.ToList();

            var result = houses.Select(h => new
            {
                h.Id,
                h.Name,
                h.Address,
                h.RentPrice,
                h.HouseType,   //  展示類型
                h.AreaSize,    //  展示坪數
                h.Status,      //  展示狀態


                FloorInfo = h.FloorInfo,
                IncludeUtilities = h.IncludeUtilities,
                IncludeWifi = h.IncludeWifi,
                IncludeManagememtFee = h.IncludeManagementFee,
                CoverUrl = _context.House_Images
                                   .FirstOrDefault(img => img.HouseId == h.Id && img.IsCover == true)?.Url,
                Images = _context.House_Images
                                 .Where(img => img.HouseId == h.Id)
                                 .Select(img => new { img.Id, img.Url, img.IsCover })
                                 .ToList()
            }).ToList();

            return Ok(result);
        }

        // 3. 取得單一房屋詳細資料
        [HttpGet("{id}")]
        public IActionResult GetHouseById(int id)
        {
            var house = _context.Rent_Houses.Find(id);
            if (house == null) return NotFound("找不到這間房子喔！");
            return Ok(house);
        }

        // ✏️ 4. 修改房屋資料 (全欄位更新)
        [HttpPut("{id}")]
        public IActionResult UpdateHouse(int id, [FromBody] CreateHouseDto request)
        {
            var house = _context.Rent_Houses.Find(id);
            if (house == null) return NotFound("找不到要修改的房子！");

            // 基本資料
            house.DistrictId = request.DistrictId;
            house.Name = request.Name;
            house.Address = request.Address;
            house.Description = request.Description;
            house.RentPrice = request.RentPrice;

            // 🆕 詳細設備與規格
            house.IncludeUtilities = request.IncludeUtilities;
            house.IncludeWifi = request.IncludeWifi;
            house.IncludeManagementFee = request.IncludeManagementFee;
            house.AreaSize = request.AreaSize;
            house.LeaseTerm = request.LeaseTerm;
            house.FloorInfo = request.FloorInfo;
            house.HouseType = request.HouseType;
            house.Status = request.Status;

            _context.SaveChanges();
            return Ok(new { Message = "房屋資料更新成功！", HouseData = house });
        }

        // 🗑️ 5. 刪除房屋
        [HttpDelete("{id}")]
        public IActionResult DeleteHouse(int id)
        {
            var house = _context.Rent_Houses.Find(id);
            if (house == null) return NotFound("找不到要刪除的房子！");

            _context.Rent_Houses.Remove(house);
            _context.SaveChanges();
            return Ok("房屋刪除成功！");
        }

        // ==========================================
        // 🖼️ 圖片操作區 (維持原樣)
        // ==========================================

        [HttpPost("Image/AddRecord")]
        public IActionResult AddImageRecord([FromBody] AddHouseImageDto request)
        {
            var newImage = new HouseImage
            {
                HouseId = request.HouseId,
                Url = request.Url,
                Description = request.Description,
                IsCover = false
            };
            _context.House_Images.Add(newImage);
            _context.SaveChanges();
            return Ok(new { Message = "照片紀錄已成功寫入！", NewImageId = newImage.Id });
        }

        [HttpPut("Image/{imageId}/SetCover")]
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

        [HttpDelete("Image/{imageId}")]
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
    }

    public class AddHouseImageDto
    {
        public int HouseId { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
    }
}
