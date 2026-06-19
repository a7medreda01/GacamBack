using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppDAL.Entities
{
    public enum OrderType
    {
        CertificatePrint,
        AccreditationCardPrint
    }

    public enum OrderStatus
    {
        Pending,
        WaitingPayment,
        PaymentSubmitted,
        UnderReview,
        Approved,
        InProduction,
        Printed,
        ReadyForDelivery,
        Delivered,
        Rejected,
        Cancelled
    }

    /// <summary>
    /// Represents a print order for certificates or accreditation cards.
    /// </summary>
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public OrderType OrderType { get; set; }

        [Required]
        public int RelatedRecordId { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public int? PaymentId { get; set; }
        public Payment? Payment { get; set; }

        [Required]
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(100)]
        public string? TrackingNumber { get; set; }
        public decimal ShippingFee { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    }
}
