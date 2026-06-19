using AppDAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace AppBL.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public OrderType OrderType { get; set; }
        public int RelatedRecordId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public int? PaymentId { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string? Notes { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateOrderDto
    {
        [Required]
        public OrderType OrderType { get; set; }

        [Required]
        public int RelatedRecordId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; } = 1;

        [MaxLength(1000)]
        public string? Notes { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public class UpdateOrderDto
    {
        [Range(1, 100)]
        public int? Quantity { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(100)]
        public string? TrackingNumber { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        [Required]
        public OrderStatus OrderStatus { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class OrderStatusHistoryDto
    {
        public int Id { get; set; }
        public OrderStatus? OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public string? ChangedByUserName { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
