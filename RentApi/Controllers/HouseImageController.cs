using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting; 

namespace RentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        
        public ImageController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // ==========================================
        // 照片儲存
        // ==========================================
        [HttpPost("Upload")]
        public async Task<IActionResult> UploadImages(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("寄物櫃沒有收到任何圖片喔！");
            }


            var folderPath = Path.Combine(_env.ContentRootPath, "wwwroot", "Uploads");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var uploadedUrls = new List<string>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    uploadedUrls.Add($"/Uploads/{fileName}");
                }
            }

            return Ok(new
            {
                Message = $"🎉 寄物櫃成功接收了 {files.Count} 張圖片！",
                Urls = uploadedUrls
            });
        }
    }
}
