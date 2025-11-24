using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using ProjectLaborBackend.Dtos.UserDTOs;
using ProjectLaborBackend.Email;
using ProjectLaborBackend.Email.Models;
using ProjectLaborBackend.Entities;
using ProjectLaborBackend.Services;

namespace ProjectLaborBackend.Controllers
{
    [Route("api/email")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;
        public EmailController(IEmailService emailService, AppDbContext context)
        {
            _emailService = emailService;
            _context = context;
        }

        [HttpPost("send-welcome")]
        public async Task<IActionResult> SendWelcomeEmail([FromBody] UserEmailDto emailDto, int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == emailDto.Email);

            try
            {
                UserModel userModel = new UserModel { User = user };

                string templatePath = $"{Directory.GetCurrentDirectory()}/Email/Templates/Welcome.cshtml";
                await _emailService.SendEmail(emailDto.Email, "Üdvözlünk a RakLapnál!", templatePath, userModel);
                return Ok("Welcome email sent successfully!");
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpPost("send-password-reset")]
        public async Task<IActionResult> SendPasswordResetEmail([FromBody] UserEmailDto emailDto, int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == emailDto.Email);
            var emailModel = new PasswordResetEmailModel
            {
                User = user,
                Token = "test test"
            };

            try
            {
                string templatePath = $"{Directory.GetCurrentDirectory()}/Email/Templates/PasswordReset.cshtml";
                await _emailService.SendEmail(emailDto.Email, "Jelszó visszaállítás", templatePath, emailModel);
                return Ok("Password reset email sent successfully!");
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
