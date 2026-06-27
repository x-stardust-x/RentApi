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
        public async Task<FAQ_Category> CreateCategoryAsync(FAQ_CategoryDto category) {
            var newCategory = new FAQ_Category {
                Name = category.Name,
                SortOrder = category.SortOrder,
                IsActive = category.IsActive
            };
            _db.FAQ_Category.Add(newCategory);
            await _db.SaveChangesAsync();
            return newCategory;
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
        public async Task<FAQ_Category?> UpdateCategoryAsync(int id, FAQ_CategoryDto category) {
            var faqCategory = await _db.FAQ_Category.FirstOrDefaultAsync(x => x.Id == id);
            if (faqCategory == null) {
                return null;
            }
            faqCategory.Name = category.Name;
            faqCategory.SortOrder = category.SortOrder;
            faqCategory.IsActive = category.IsActive;
            await _db.SaveChangesAsync();
            return faqCategory;
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
        public async Task<string> DeleteCategoryAndItemAsync(int categoryId) {
            var transaction = await _db.Database.BeginTransactionAsync();
            try {
                var category = await _db.FAQ_Category.FirstOrDefaultAsync(x => x.Id == categoryId);
                if (category == null) {
                    return "FAQ category not found.";
                }
                var faqItems = await _db.FAQ_Item.Where(x => x.CategoryId == category.Id).ToListAsync();
                _db.FAQ_Item.RemoveRange(faqItems);
                await _db.SaveChangesAsync();
                _db.FAQ_Category.Remove(category);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
                return "FAQ category and items deleted successfully.";
            }
            catch (Exception err) {
                await transaction.RollbackAsync();
                return "Has Error +" + err.Message;
            }
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
