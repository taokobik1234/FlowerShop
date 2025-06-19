// DTOs/Request/User/UpdateUserRoleDto.cs
using System.ComponentModel.DataAnnotations;

namespace BackEnd_FLOWER_SHOP.DTOs.Request.User
{
    public class UpdateUserRoleDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; } // Assuming UserId is a string (GUID)

        [Required(ErrorMessage = "New role name is required")]
        [StringLength(50, ErrorMessage = "Role name cannot exceed 50 characters")]
        public string NewRoleName { get; set; } // e.g., "Admin", "User"
    }
}