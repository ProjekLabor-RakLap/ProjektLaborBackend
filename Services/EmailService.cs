using AutoMapper;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using ProjectLaborBackend.Dtos.UserDTOs;
using ProjectLaborBackend.Email;
using ProjectLaborBackend.Email.Models;
using ProjectLaborBackend.Entities;

namespace ProjectLaborBackend.Services
{
    public interface IEmailService
    {
        Task SendEmail<T>(string userEmail, string subject, string template, T model);
    }
    public class EmailService : IEmailService
    {
        private IFluentEmailFactory _fluentEmail;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public EmailService(IFluentEmailFactory fluentEmail, AppDbContext context, IMapper mapper)
        {
            _fluentEmail = fluentEmail;
            _context = context;
            _mapper = mapper;
        }

        public async Task SendEmail<T>(string userEmail, string subject, string template, T model)
        {
            var UserDTO = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            
            if (UserDTO == null)
            {
                throw new KeyNotFoundException("User with given email does not exist!");
            }

            await _fluentEmail
               .Create()
               .To(userEmail)
               .Subject(subject)
               .UsingTemplateFromFile(template, model)
               .SendAsync();
        }
    }
}
