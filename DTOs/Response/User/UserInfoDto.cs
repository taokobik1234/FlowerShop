using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.DTOs.Response.User
{
    public class UserInfoDto
    {
        public long Id { get; set; }

        public string UserName { get; set; }

        public string Role { get; set; }

    }
}