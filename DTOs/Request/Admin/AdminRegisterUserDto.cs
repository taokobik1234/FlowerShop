// DTOs/Request/User/AdminRegisterUserDto.cs
using System.ComponentModel.DataAnnotations;

namespace BackEnd_FLOWER_SHOP.DTOs.Request.User
{
    public class AdminRegisterUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string UserName { get; set; }

        public string? PhoneNumber { get; set; }

        [Required]
        public string RoleName { get; set; } // To specify the role (e.g., "Admin", "User")
    }
}