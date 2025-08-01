using System;

namespace BackEnd_FLOWER_SHOP.DTOs.Response.Product
{
    public class ReviewResponseDto
    {
        public long Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}