namespace Order.API.OrderServices;

public class Order
{
    public int Id { get; set; }
    public string OrderCode { get; set; }
    public DateTime Created { get; set; }
    public int UserId { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItem> Items { get; set; }
}

public enum OrderStatus : byte
{
    Success=1,
    Fail=0
}

public class OrderItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Count { get; set; }
    public decimal UnitPrice { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
}