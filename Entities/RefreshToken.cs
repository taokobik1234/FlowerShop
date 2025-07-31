using System.ComponentModel.DataAnnotations;

namespace BackEnd_FLOWER_SHOP.Entities
{
    public class RefreshToken
    {
        public long Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Token { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public bool IsRevoked { get; set; }

        [Required]
        public long UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}