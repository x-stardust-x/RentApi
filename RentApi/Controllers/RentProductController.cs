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
                .Where(p => p.Status == 1) // 前台只看得到已上架的
                .Select(p => new
                {
                    p.Id,
                    p.AccountId,
                    p.Name,
                    p.Category,
                    p.Price,
                    p.PriceUnit,
                    p.Status,
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

                             
                          join acc in _context.Account on p.AccountId equals acc.Id into accounts
                          from acc in accounts.DefaultIfEmpty()

                          select new
                          {
                              p.Id,
                              p.Name,
                              p.Category,
                              p.Price,
                              p.PriceUnit,
                              p.Description,
                              p.Address,
                              p.Status,
                              CoverUrl = _context.Product_Image.Where(img => img.ProductId == p.Id && img.IsCover).Select(img => img.Url).FirstOrDefault(),

                             
                              UserName = acc != null ? acc.Username : "神祕會員",
                              UserAvatar = ""
                          }).ToList();

            return Ok(result);
        }


        // ==========================================
        // ✨ 3. 新增 (Create)
        // ==========================================
        [HttpPost]
        public IActionResult CreateProduct([FromBody] CreateRentProductDto request)
        {
            if (request == null) return BadRequest("資料不能為空");

            // 🚀 1. 防呆檢查：確保前端有傳入有效的 AccountId
            if (request.AccountId <= 0)
            {
                return BadRequest($"無效的使用者 ID！接收到的 AccountId 為: {request.AccountId}，請檢查前端 Angular 傳送的資料。");
            }

            // 🚀 2. 關聯檢查：去資料庫確認這個會員真的存在 (這一步可視需求決定要不要加)
            var accountExists = _context.Account.Any(a => a.Id == request.AccountId);
            if (!accountExists)
            {
                return NotFound($"找不到 ID 為 {request.AccountId} 的會員，請確認該會員是否已註冊！");
            }

            var newProduct = new RentProduct
            {
                AccountId = request.AccountId,
                Name = request.Name,
                Category = request.Category,
                Description = request.Description,
                Price = request.Price,
                PriceUnit = request.PriceUnit,
                Deposit = request.Deposit,
                Status = 0,
                Quantity = request.Quantity,
                OwnTool = request.OwnTool,
                RequiredKnowledge = request.RequiredKnowledge,
                UsageRequirements = request.UsageRequirements,
                UsageTerms = request.UsageTerms,
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
            product.Status = request.Status;
            product.Quantity = request.Quantity;
            product.OwnTool = request.OwnTool;
            product.UsageRequirements = request.UsageRequirements;
            product.UsageTerms = request.UsageTerms;
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

            var relatedImages = _context.Product_Image.Where(img => img.ProductId == id).ToList();


            if (relatedImages.Any())
            {
                _context.Product_Image.RemoveRange(relatedImages);
            }


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

        public async Task<IActionResult> ApproveProductStatus(int id)
        {
            var product = await _context.Rent_Products.FindAsync(id);
            if (product == null) return NotFound("找不到這個資產/技能喔！");

            product.Status = 1;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "資產/技能核准成功！" });
        }


        [HttpDelete("TakeDown/{id}")]
        public async Task<IActionResult> TakeDownProductStatus(int id)
        {
            try
            {
                var product = await _context.Rent_Products.FindAsync(id);
                if (product == null)
                    return NotFound(new { Message = "找不到這個資產/技能喔！" });


                var relatedImages = _context.Product_Image.Where(img => img.ProductId == id).ToList();
                if (relatedImages.Any())
                {
                    _context.Product_Image.RemoveRange(relatedImages);
                }


                _context.Rent_Products.Remove(product);

                await _context.SaveChangesAsync();
                return Ok(new { Message = "資產/技能已徹底刪除！" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "系統發生錯誤，請稍後再試。", Details = ex.Message });
            }
        }

        // ==========================================
        // 🔍 取得單一資產/技能詳細資料 (終極安全防護版)
        // ==========================================
        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            // 1. 只撈取文字欄位，不使用 .Include 避免資料庫物件連鎖崩潰
            var p = _context.Rent_Products.FirstOrDefault(x => x.Id == id);

            if (p == null) return NotFound("找不到該項資產或技能！");

            // 2. 乾淨撈取照片清單
            var oldPhotos = _context.Product_Image
                                    .Where(img => img.ProductId == id)
                                    .Select(img => new {
                                        img.Id,
                                        img.ProductId,
                                        img.Url,
                                        img.Description,
                                        img.IsCover
                                    }).ToList();

            // 3. 🌟 終極安全牌：手動解構所有欄位！
            // 這樣可以 100% 確保轉成 JSON 時是乾淨的 camelCase，且絕對不會有循環參考問題
            return Ok(new
            {
                Product = new
                {
                    p.Id,
                    p.AccountId,
                    p.Name,
                    p.Category,
                    p.Description,
                    p.Price,
                    p.PriceUnit,
                    p.Deposit,
                    p.Status,
                    p.Quantity,
                    p.OwnTool,
                    p.RequiredKnowledge,
                    p.UsageRequirements,
                    p.UsageTerms,
                    p.Address
                },
                Images = oldPhotos
            });
        }

        [HttpGet("User/{accountId:int}")]
        public async Task<IActionResult> GetProductsByAccountId(int accountId)
        {
            try
            {
                var products = await _context.Rent_Products
                    .AsNoTracking()
                    .Where(p => p.AccountId == accountId)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Category,
                        p.Price,
                        p.PriceUnit,
                        p.Description,
                        p.Address,
                        p.Status,
                        p.Deposit,
                        p.Quantity,
                        p.OwnTool,
                        p.RequiredKnowledge,
                        p.UsageRequirements,
                        p.UsageTerms
                    })
                    .ToListAsync();

                var productIds = products
                    .Select(p => p.Id)
                    .ToList();

                var coverImages = await _context.Product_Image
                    .AsNoTracking()
                    .Where(img => productIds.Contains(img.ProductId) && img.IsCover)
                    .Select(img => new
                    {
                        img.ProductId,
                        img.Url
                    })
                    .ToListAsync();

                var coverMap = coverImages
                    .GroupBy(img => img.ProductId)
                    .ToDictionary(
                        group => group.Key,
                        group => group.First().Url
                    );

                var result = products.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Category,
                    p.Price,
                    p.PriceUnit,
                    p.Description,
                    p.Address,
                    p.Status,
                    p.Deposit,
                    p.Quantity,
                    p.OwnTool,
                    p.RequiredKnowledge,
                    p.UsageRequirements,
                    p.UsageTerms,

                    CoverUrl = coverMap.ContainsKey(p.Id)
                        ? coverMap[p.Id]
                        : null
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "取得工具 / 技能列表失敗",
                    Details = ex.Message,
                    Inner = ex.InnerException?.Message
                });
            }
        }
    }
}