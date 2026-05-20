using Microsoft.AspNetCore.Mvc;
using RentApi.Data;
using RentApi.Models;

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
    }
}

