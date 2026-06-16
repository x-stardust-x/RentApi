using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using RentApi.Models;

namespace RentApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Admin> Admin { get; set; }
        public DbSet<Account> Account { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<User_Habit> User_Habit { get; set; }

        public DbSet<Rent_House> Rent_Houses { get; set; }
        public DbSet<HouseImage> House_Images { get; set; }

        public DbSet<Location_District> Location_Districts { get; set; }

        public DbSet<RentProduct> Rent_Products { get; set; }

        public DbSet<ProductImage> Product_Image { get; set; }
        public DbSet<HouseRules> HouseRules { get; set; }

        public DbSet<ContactUs> ContactUs { get; set; }

        public DbSet<HouseFacility> HouseFacilities { get; set; }
        public DbSet<SystemFacility> SystemFacilities { get; set; }

        public DbSet<City> City { get; set; }
        //public DbSet<Location_District> Location_District { get; set; }

        public DbSet<System_Announcement> System_Announcement { get; set; }
        public DbSet<System_Log> System_Log { get; set; }
        public DbSet<FAQ_Category> FAQ_Category { get; set; }
        public DbSet<FAQ_Item> FAQ_Item { get; set; }

        public DbSet<HouseViewing> HouseViewings { get; set; }
        public DbSet<HouseViewingAvailableSlot> HouseViewingAvailableSlots { get; set; }

        public DbSet<FavoriteHouse> FavoriteHouses { get; set; }
    }
}