using System.ComponentModel.DataAnnotations;

namespace ProjectLaborBackend.Dtos.UserDTOs
{
    public class UserRegisterDTO
    {
        [Required]
        [StringLength(75)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(75)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        public int RoleId { get; set; }
    }
}
