using System.ComponentModel.DataAnnotations;
using AppDAL.Entities;

namespace AppBL.DTOs
{
    public class ServiceFeeDto
    {
        public int Id { get; set; }

        public OrderType OrderType { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal ShippingFee { get; set; }

        public bool IsActive { get; set; }
    }

    public class ServiceFeeUpdateRequest
    {
        [Range(0, 10000)]
        public decimal UnitPrice { get; set; }

        [Range(0, 10000)]
        public decimal ShippingFee { get; set; }

        public bool IsActive { get; set; }
    }
}
