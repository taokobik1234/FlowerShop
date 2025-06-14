using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.DTOs.Response.User
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public UserInfoDto User { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}