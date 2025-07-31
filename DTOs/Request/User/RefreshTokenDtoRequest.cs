using System.ComponentModel.DataAnnotations;

namespace BackEnd_FLOWER_SHOP.DTOs.Request.User
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; }
    }
}