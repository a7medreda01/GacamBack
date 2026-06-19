using System.ComponentModel.DataAnnotations;

namespace AppDAL.Entities
{
    /// <summary>
    /// Tracks order status changes for timeline/history views.
    /// </summary>
    public class OrderStatusHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public OrderStatus? OldStatus { get; set; }

        [Required]
        public OrderStatus NewStatus { get; set; }

        public int? ChangedByUserId { get; set; }
        public User? ChangedByUser { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
