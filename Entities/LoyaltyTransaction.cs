using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd_FLOWER_SHOP.Entities
{
    // This is a new entity to track the history of all point changes.
    // It provides an audit trail for the loyalty system.
    public class LoyaltyTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PointsChange { get; set; }

        [Required]
        [MaxLength(250)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
