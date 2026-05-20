using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RentApi.Models.DTO;
using RentApi.Services;

namespace RentApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class FAQController : ControllerBase {
        private readonly FAQService _service;
        public FAQController(FAQService service) {
            _service = service;
        }
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories() {
            var faqItems = await _service.GetCategoriesAsync();
            if (faqItems.Count == 0) {
                return NotFound("No FAQ categories found.");
            }
            return Ok(faqItems);
        }
        [HttpGet("FAQ_Items")]
        public async Task<IActionResult> GetFAQItems() {
            var categories = await _service.GetAllAsync();
            if (categories.Count == 0) {
                return NotFound("No FAQ items found.");
            }
            return Ok(categories);
        }
        [HttpGet("categories/{categoryId}")]
        public async Task<IActionResult> GetByCategoryId(int categoryId) {
            var faqItems = await _service.GetItemsByCategoryIdAsync(categoryId);
            if (faqItems.Count == 0) {
                return NotFound("No FAQ items found for this category.");
            }
            return Ok(faqItems);
        }
        [HttpPost]
        public async Task<IActionResult> CreateFAQItem(FAQDto item) {
            var faqItem = await _service.CreateFAQItemAsync(item);
            if (faqItem == null) {
                return BadRequest("Failed to create FAQ item.");
            }
            return CreatedAtAction(nameof(GetFAQItems), new { id = faqItem.Id }, faqItem);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFAQItem(int id, FAQDto item) {
            var faqItem = await _service.UpdateFAQItemAsync(id, item);
            if (faqItem == null) {
                return NotFound("FAQ item not found.");
            }
            return Ok(faqItem);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFAQItem(int id) {
            var result = await _service.DeleteFAQItemAsync(id);
            return Ok(result);
        }
    }
}
