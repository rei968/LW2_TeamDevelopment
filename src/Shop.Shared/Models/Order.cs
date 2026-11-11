using Shop.Shared.Enums;

namespace Shop.Shared.Models;

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);

    // Властивість для демонстрації merge-конфлікту:
    public string StatusDisplay => $"Статус: {Status}";
}
