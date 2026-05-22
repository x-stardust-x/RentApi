//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using RentApi.Models;
//using RentApi.Models.DTO;

//namespace RentApi.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class RentItemController : ControllerBase
//    {
//        private readonly AppDbContext _context;

//        public RentItemController(AppDbContext context)
//        {
//            _context = context;
//        }

//        // 1. 房東上架新用品
//        [HttpPost]
//        public async Task<IActionResult> CreateItem([FromBody] CreateRentProductsDto dto)
//        {
//            // 💡 這裡測試時先寫死 AccountId = 1，之後整合登入再換成 Token 取值
//            var newItem = new Rent_product
//            {
//                AccountId = 1,
//                Name = dto.Name,
//                Category = dto.Category,
//                Description = dto.Description,
//                Price = dto.Price,
//                PriceUnit = dto.PriceUnit
//            };

//            _context.RentItems.Add(newItem);
//            await _context.SaveChangesAsync();

//            return Ok(new { message = "🎉 用品上架成功！", itemId = newItem.Id });
//        }

//        // 2. 取得所有出租用品
//        [HttpGet]
//        public async Task<IActionResult> GetAllItems()
//        {
//            var items = await _context.RentItems.ToListAsync();
//            return Ok(items);
//        }
//    }
//}
