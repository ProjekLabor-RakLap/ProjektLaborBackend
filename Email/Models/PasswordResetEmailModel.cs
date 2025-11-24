using ProjectLaborBackend.Entities;

namespace ProjectLaborBackend.Email.Models
{
    public class PasswordResetEmailModel
    {
        public User User { get; set; }
        public string Token { get; set; }
    }
}
