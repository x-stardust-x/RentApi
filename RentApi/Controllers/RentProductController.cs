using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentApi.Data;
using RentApi.Models;
using RentApi.Models.DTO;
using System;
using System.Linq;

namespace CoLiving.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentProductController : Controller
    {
        private readonly AppDbContext _context;

        public RentProductController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 🔍 1. 查詢列表 (一般前台用)
        // ==========================================
        [HttpGet]
        public IActionResult GetAllProducts()
        {
            var products = _context.Rent_Products
                .Where(p => p.IsOnline == true) // 前台只看得到已上架的
                .Select(p => new
                {
                    p.Id,
                    p.AccountId,
                    p.Name,
                    p.Category,
                    p.Price,
                    p.PriceUnit,
                    p.IsOnline,
                    p.Quantity,
                    p.CreatedAt
                }).ToList();

            return Ok(products);
        }

        // ==========================================
        // 🌟 1-2. 查詢列表 (給管理員後台專用，含封面圖與所有狀態)
        // ==========================================
        [HttpGet("AllForAdmin")]
        public IActionResult GetAllProductsForAdmin()
        {
            var result = (from p in _context.Rent_Products
                          select new
                          {
                              p.Id,
                              p.Name,
                              p.Category,
                              p.Price,
                              p.PriceUnit,
                              p.Description,
                              p.Address,
                              p.IsOnline, // 🌟 前端過濾就靠這個！
                              CoverUrl = _context.Product_Image.Where(img => img.ProductId == p.Id && img.IsCover).Select(img => img.Url).FirstOrDefault()
                          }).ToList();
            return Ok(result);
        }

        // ==========================================
        // 🔍 2. 查詢單筆詳細資料 (Read One)
        // ==========================================
        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            var product = _context.Rent_Products.Find(id);
            if (product == null) return NotFound("找不到該項資產或技能！");
            return Ok(product);
        }

        // ==========================================
        // ✨ 3. 新增 (Create)
        // ==========================================
        [HttpPost]
        public IActionResult CreateProduct([FromBody] CreateRentProductDto request)
        {
            if (request == null) return BadRequest("資料不能為空");

            var newProduct = new RentProduct
            {
                AccountId = request.AccountId,
                Name = request.Name,
                Category = request.Category,
                Description = request.Description,
                Price = request.Price,
                PriceUnit = request.PriceUnit,
                Deposit = request.Deposit,
                IsOnline = false,
                Quantity = request.Quantity,
                OwnTool = request.OwnTool,
                RequiredKnowledge = request.RequiredKnowledge,
                Address = request.Address,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Rent_Products.Add(newProduct);
            _context.SaveChanges();

            return Ok(new { Message = "建立成功！", Product = newProduct });
        }

        // ==========================================
        // ✏️ 4. 修改 (Update)
        // ==========================================
        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] CreateRentProductDto request)
        {
            var product = _context.Rent_Products.Find(id);
            if (product == null) return NotFound("找不到要修改的資料！");

            product.Name = request.Name;
            product.Category = request.Category;
            product.Description = request.Description;
            product.Price = request.Price;
            product.PriceUnit = request.PriceUnit;
            product.Deposit = request.Deposit;
            product.IsOnline = request.IsOnline;
            product.Quantity = request.Quantity;
            product.OwnTool = request.OwnTool;
            product.RequiredKnowledge = request.RequiredKnowledge;
            product.Address = request.Address;
            product.UpdatedAt = DateTime.Now;

            _context.SaveChanges();
            return Ok(new { Message = "資料更新成功！", Product = product });
        }

        // ==========================================
        // 🗑️ 5. 刪除 (Delete)
        // ==========================================
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Rent_Products.Find(id);
            if (product == null) return NotFound("找不到要刪除的資料！");

            _context.Rent_Products.Remove(product);
            _context.SaveChanges();
            return Ok(new { Message = "資料已成功刪除！" });
        }

        // ==========================================
        // 🖼️ 圖片操作區
        // ==========================================
        [HttpPost("Image/AddRecord")]
        public IActionResult AddProductImageRecord([FromBody] AddProductImageDto request)
        {
            if (request == null) return BadRequest("請求資料不能為空喔！");
            var newImage = new ProductImage
            {
                ProductId = request.ProductId,
                Url = request.Url,
                Description = request.Description,
                IsCover = request.IsCover
            };
            _context.Product_Image.Add(newImage);
            _context.SaveChanges();
            return Ok(new { Message = "🎉 圖片關聯成功寫入資料庫！", ImageId = newImage.Id });
        }

        [HttpPut("Image/{imageId}/SetCover")]
        public IActionResult SetProductCoverImage(int imageId)
        {
            var targetImage = _context.Product_Image.Find(imageId);
            if (targetImage == null) return NotFound("找不到照片");

            var allImages = _context.Product_Image.Where(img => img.ProductId == targetImage.ProductId).ToList();
            foreach (var img in allImages) img.IsCover = false;

            targetImage.IsCover = true;
            _context.SaveChanges();
            return Ok(new { Message = "🎉 資產首圖設定成功！" });
        }

        [HttpDelete("Image/{imageId}")]
        public IActionResult DeleteProductImage(int imageId)
        {
            var image = _context.Product_Image.Find(imageId);
            if (image == null) return NotFound();

            var fileName = System.IO.Path.GetFileName(image.Url);
            var filePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "Uploads", fileName);

            _context.Product_Image.Remove(image);
            _context.SaveChanges();

            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            return Ok(new { Message = "照片刪除成功！" });
        }

        // ==========================================
        // 🛠️ 資產 / 技能 審核與下架專區
        // ==========================================
        [HttpPut("Approve/{id}")]
        public IActionResult ApproveProductStatus(int id)
        {
            var product = _context.Rent_Products.Find(id);
            if (product == null) return NotFound("找不到這個資產/技能喔！");

            product.IsOnline = true; // 核准上架
            _context.SaveChanges();
            return Ok(new { Message = "資產/技能核准成功！" });
        }

        [HttpPut("TakeDown/{id}")]
        public IActionResult TakeDownProductStatus(int id)
        {
            var product = _context.Rent_Products.Find(id);
            if (product == null) return NotFound("找不到這個資產/技能喔！");

            product.IsOnline = false; // 退回或強制下架
            _context.SaveChanges();
            return Ok(new { Message = "資產/技能已強制下架！" });
        }
    }
}