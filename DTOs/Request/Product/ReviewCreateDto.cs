using System.ComponentModel.DataAnnotations;

namespace BackEnd_FLOWER_SHOP.DTOs.Request.Product
{
    public class ReviewCreateDto
    {
        [Required]
        public long ProductId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; }
    }
}