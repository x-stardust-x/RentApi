using Microsoft.EntityFrameworkCore;
using RentApi.Data;
using RentApi.Models;
using RentApi.Models.DTO;

namespace RentApi.Services {
    public class FAQService {
        private readonly AppDbContext _db;
        public FAQService(AppDbContext db) {
            _db = db;
        }
        public async Task<List<FAQ_Category>> GetCategoriesAsync() {
            return await _db.FAQ_Category
                .OrderBy(x => x.Id)
                .ToListAsync();
        }
        public async Task<List<FAQ_Item>> GetAllAsync() {
            return await _db.FAQ_Item
                .OrderBy(x => x.Id)
                .ToListAsync();
        }
        public async Task<List<FAQ_Item>> GetItemsByCategoryIdAsync(int categoryId) {
            return await _db.FAQ_Item
                .Where(x => x.CategoryId == categoryId)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }
        public async Task<FAQ_Item?> CreateFAQItemAsync(FAQDto item) {
            var faqItem = new FAQ_Item {
                CategoryId = item.CategoryId,
                Question = item.Question,
                Answer = item.Answer,
                SortOrder = item.SortOrder,
                Status = item.Status,
                CreatedAt = DateTime.Now
            };
            _db.FAQ_Item.Add(faqItem);
            await _db.SaveChangesAsync();
            return faqItem;
        }
        public async Task<FAQ_Item?> UpdateFAQItemAsync(int id, FAQDto item) {
            var faqItem = await _db.FAQ_Item.FirstOrDefaultAsync(x => x.Id == id);
            if (faqItem == null) {
                return null;
            }
            faqItem.CategoryId = item.CategoryId;
            faqItem.Question = item.Question;
            faqItem.Answer = item.Answer;
            faqItem.SortOrder = item.SortOrder;
            faqItem.Status = item.Status;
            faqItem.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return faqItem;
        }
        public async Task<string> DeleteFAQItemAsync(int id) {
            try {
                var faqItem = await _db.FAQ_Item.FirstOrDefaultAsync(x => x.Id == id);
                if (faqItem == null) {
                    return "FAQ item not found.";
                }
                _db.FAQ_Item.Remove(faqItem);
                await _db.SaveChangesAsync();
                return "FAQ item deleted successfully.";
            }
            catch (Exception err) {
                return "Has Error +" + err.Message;
            }
        }
    }
}
