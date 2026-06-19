using AppDAL.Entities;

public class ServiceFee
{
    public int Id { get; set; }

    public OrderType OrderType { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal ShippingFee { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}