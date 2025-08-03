using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.DTOs.Request.Token
{
    public class RefreshTokenResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public List<string>? Errors { get; set; }
    }
}