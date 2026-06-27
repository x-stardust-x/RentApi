using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RentApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env) {
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file) {
            if (file == null || file.Length == 0) {
                return BadRequest("沒有檔案");
            }

            // 副檔名
            var extension = Path.GetExtension(file.FileName);

            // 新檔名
            var fileName = $"{Guid.NewGuid()}{extension}";

            // uploads 資料夾
            var folderPath = Path.Combine(
                _env.WebRootPath,
                "uploads"
            );

            // 如果不存在就建立
            if (!Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath);
            }

            // 完整路徑
            var filePath = Path.Combine(folderPath, fileName);

            // 寫入檔案
            using var stream = new FileStream(filePath, FileMode.Create);

            await file.CopyToAsync(stream);

            // 回傳網址
            var url = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";

            return Ok(new {
                url
            });
        }
    }
}
