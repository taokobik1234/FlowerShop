using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.DTOs.Request.Product
{
    public class ImageResponseDto
    {
        public long Id { get; set; }
        public string ImageUrl { get; set; }
        public string PublicId { get; set; }
    }
}