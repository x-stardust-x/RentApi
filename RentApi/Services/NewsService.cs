using Microsoft.EntityFrameworkCore;
using RentApi.Data;
using RentApi.Models;
using RentApi.Models.DTO;

namespace RentApi.Services {
    public class NewsService {
        public AppDbContext _db;
        public NewsService(AppDbContext db) {
            _db = db;
        }
        public async Task<List<System_Announcement>?> GetAllAsync() {
            var res = await _db.System_Announcement.ToListAsync();
            return res;
        }

        public async Task<System_Announcement?> GetByIdAsync(int id) {
            var res = await _db.System_Announcement.FirstOrDefaultAsync(x => x.Id == id);
            return res;
        }

        public async Task<System_Announcement> PostAsync(System_AnnouncementDto announcement) {
            var res = new System_Announcement();
            res.AdminId = announcement.AdminId;
            res.Category = announcement.Category;
            res.Title = announcement.Title;
            res.Cover = announcement.Cover;
            res.Intro = announcement.Intro;
            res.Content = announcement.Content;
            res.SEOTitle = announcement.SEOTitle;
            res.SEODesc = announcement.SEODesc;
            res.Status = announcement.Status;
            res.CreatedAt = DateTime.Now;
            res.UpdatedAt = DateTime.Now;
            _db.System_Announcement.Add(res);
            await _db.SaveChangesAsync();
            return res;
        }
        public async Task<System_AnnouncementDto?> PutAsync(int id, System_AnnouncementDto announcement) {
            var res = await _db.System_Announcement.FirstOrDefaultAsync(x => x.Id == id);
            if (res == null)
                return null;
            res.AdminId = announcement.AdminId;
            res.Category = announcement.Category;
            res.Title = announcement.Title;
            res.Cover = announcement.Cover;
            res.Intro = announcement.Intro;
            res.Content = announcement.Content;
            res.SEOTitle = announcement.SEOTitle;
            res.SEODesc = announcement.SEODesc;
            res.Status = announcement.Status;
            res.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return announcement;
        }
        public async Task<System_Announcement?> DeleteAsync(int id) {
            var res = await _db.System_Announcement.FirstOrDefaultAsync(x => x.Id == id);
            if (res == null)
                return null;
            _db.System_Announcement.Remove(res);
            await _db.SaveChangesAsync();
            return res;
        }
    }
}
