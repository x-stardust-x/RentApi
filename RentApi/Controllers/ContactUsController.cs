using Microsoft.AspNetCore.Mvc;
using RentApi.Data;
using RentApi.Models;
using System.Net;
using System.Net.Mail;

namespace RentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ContactController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult SubmitMessage([FromBody] AddContactDto request)
        {

            var newMessage = new ContactUs
            {
                ContactName = request.Name,
                Email = request.Email,
                Subject = request.Subject,
                Content = request.Content,


                Status = 0,
                CreateAt = DateTime.Now
            };


            _context.ContactUs.Add(newMessage);
            _context.SaveChanges();


            return Ok(new { Message = "您的訊息已成功送出，我們會盡快與您聯繫！" });
        }
        [HttpGet]
        public IActionResult GetMessages()
        {
            // 從資料庫抓取所有留言，並依照時間由新到舊排序
            var messages = _context.ContactUs
                                   .OrderByDescending(m => m.CreateAt)
                                   .ToList();

            return Ok(messages);
        }
        [HttpPost("Reply")]
        public IActionResult ReplyMessage([FromBody] ReplyDto request)
        {
            // 1. 從資料庫把這筆客人留言找出來
            var message = _context.ContactUs.Find(request.Id);
            if (message == null)
            {
                return NotFound(new { Message = "找不到這筆留言，無法回信。" });
            }

            try
            {
                // 2. 開始準備信件 (MailMessage)
                using (MailMessage mail = new MailMessage())
                {
                    // 🌟 這裡換成你用來發信的 Gmail 帳號
                    mail.From = new MailAddress("lai88820@gmail.com");

                    // 收件人：直接抓資料庫裡客人當初留的 Email
                    mail.To.Add(message.Email);

                    // 信件標題：加上前綴讓客人知道這是客服回覆
                    mail.Subject = "【RentHouse】客服回覆：" + message.Subject;

                    // 信件內容：就是你在後台打的那些字
                    mail.Body = request.ReplyContent;
                    mail.IsBodyHtml = false; // 純文字信件

                    // 3. 呼叫郵差 (SmtpClient) 幫我們送信
                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        
                        smtp.Credentials = new NetworkCredential("lai88820@gmail.com", "nsbz hhci bhxb dlgq  ");
                        smtp.EnableSsl = true; 

                        smtp.Send(mail); 
                    }
                }

                
                message.Status = 1;
                _context.SaveChanges();

                return Ok(new { Message = "回信已成功寄出，狀態已更新為【已處理】！" });
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, new { Message = "寄信失敗：" + ex.Message });
            }
        }
        // 處理刪除的 API
        [HttpDelete("{id}")]
        public IActionResult DeleteMessage(int id)
        {
           
            var message = _context.ContactUs.Find(id);
            if (message == null)
            {
                return NotFound(new { Message = "找不到這筆留言，可能已經被刪除了。" });
            }

            
            _context.ContactUs.Remove(message);
            _context.SaveChanges();

            return Ok(new { Message = "留言已成功刪除！" });
        }
        [HttpPost("BatchDeiete")]
        // 🌟 批次刪除 API
        [HttpPost("BatchDelete")]
        public IActionResult BatchDelete([FromBody] List<int> ids)
        {
            // 檢查有沒有傳 ID 過來
            if (ids == null || !ids.Any())
            {
                return BadRequest(new { Message = "沒有選擇任何資料" });
            }

            // 1. 把 ID 有在名單裡的留言全部抓出來
            var messagesToDelete = _context.ContactUs.Where(m => ids.Contains(m.Id)).ToList();

            // 2. 呼叫 RemoveRange 一次全部刪除
            _context.ContactUs.RemoveRange(messagesToDelete);
            _context.SaveChanges();

            return Ok(new { Message = $"成功刪除了 {messagesToDelete.Count} 筆資料！" });
        }
    }
}

public class ReplyDto
{
    public int Id { get; set; }              // 你要回覆哪一筆留言 (靠 ID 認人)
    public string ReplyContent { get; set; } // 你在後台打的回信內容
}