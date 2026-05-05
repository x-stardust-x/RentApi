using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using RentApi.Models;

namespace RentApi.Data {
    public class AppDbContext :DbContext {
        public AppDbContext(DbContextOptions options) : base(options) {
        }
        public DbSet<Admin> Admin {  get; set; }
        public DbSet<Account> Account { get; set; }
        public DbSet<User> User { get; set; }
    }
}
