using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentApi.Data;
using RentApi.Models;
using System.Linq;

namespace CoLiving.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoriteController(AppDbContext context)
        {
            _context = context;
        }

       
        //  1. 切換收藏狀態 (存在就刪除，不存在就新增)
       
        [HttpPost("Toggle")]
        public IActionResult ToggleFavorite([FromBody] ToggleFavoriteDto request)
        {
            if (request == null || request.AccountId <= 0 || request.HouseId <= 0)
            {
                return BadRequest("要求的參數不正確");
            }

           
            var existingFav = _context.FavoriteHouses
                .FirstOrDefault(f => f.AccountId == request.AccountId && f.HouseId == request.HouseId);

            if (existingFav != null)
            {
                
                _context.FavoriteHouses.Remove(existingFav);
                _context.SaveChanges();
                return Ok(new { IsFavorite = false, Message = "已從收藏清單中移除" });
            }
            else
            {
                
                var newFav = new FavoriteHouse
                {
                    AccountId = request.AccountId,
                    HouseId = request.HouseId
                };
                _context.FavoriteHouses.Add(newFav);
                _context.SaveChanges();
                return Ok(new { IsFavorite = true, Message = "成功加入收藏清單" });
            }
        }

       
        //  撈取專屬會員的「收藏房屋清單」
        
        [HttpGet("MyList/{accountId:int}")]
        public IActionResult GetMyFavorites(int accountId)
        {
            
            var favoriteList = (from fav in _context.FavoriteHouses
                                where fav.AccountId == accountId
                                join h in _context.Rent_Houses on fav.HouseId equals h.Id

                               
                                join loc in _context.Location_Districts on h.DistrictId equals loc.DistrictId into locations
                                from loc in locations.DefaultIfEmpty()

                                   
                                join acc in _context.Account on h.AccountId equals acc.Id into accounts
                                from acc in accounts.DefaultIfEmpty()

                                select new
                                {
                                    h.Id,
                                    h.Name,
                                    h.Address,
                                    h.RentPrice,
                                    h.HouseType,
                                    h.AreaSize,
                                    h.Status,
                                    Description = h.Description,
                                    CityName = loc != null ? loc.CityName : "",
                                    DistrictName = loc != null ? loc.DistrictName : "",
                                    UserName = acc != null ? acc.Username : "神祕房東",
                                    UserAvatar = "", 

                                   
                                    CoverUrl = _context.House_Images.Where(img => img.HouseId == h.Id && img.IsCover).Select(img => img.Url).FirstOrDefault(),

                                    IsFavorite = true 
                                }).ToList();

            return Ok(favoriteList);
        }

        [HttpGet("Check/{accountId:int}/{houseId:int}")]
        public IActionResult CheckFavorite(int accountId, int houseId)
        {
            bool isFav = _context.FavoriteHouses.Any(f => f.AccountId == accountId && f.HouseId == houseId);
            return Ok(new { isFavorite = isFav });
        }
    }
}