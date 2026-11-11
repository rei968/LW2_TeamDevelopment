using Shop.Shared.Models;
using Shop.Shared.Enums;

namespace Shop.Shared.Services;

public class ShopService
{
    private static ShopService? _instance;
    public static ShopService Instance => _instance ??= new ShopService();

    // Початкові тестові дані каталогу
    public List<Product> Products { get; set; } = new()
    {
        new() { Id = 1, Name = "Ноутбук Lenovo IdeaPad", Category = "Ноутбуки", Price = 24999m, StockQuantity = 6 },
        new() { Id = 2, Name = "Смартфон Samsung Galaxy", Category = "Смартфони", Price = 17999m, StockQuantity = 10 },
        new() { Id = 3, Name = "Навушники Sony WH-1000", Category = "Аксесуари", Price = 3999m, StockQuantity = 15 }
    };

    public List<Order> Orders { get; set; } = new();

    // TODO (Задача Же): Реалізувати збереження та читання з файлу JSON (System.Text.Json)
    public void SaveToFile(string path = "shop_data.json")
    {
        // Тімлід додає код збереження у файл тут
    }

    public void LoadFromFile(string path = "shop_data.json")
    {
        // Тімлід додає код завантаження з файлу тут
    }

    // TODO (Задача Же): Доопрацювати валідацію замовлення
    public Order CreateOrder(string customerName, string phone, List<OrderItem> items)
    {
        foreach (var item in items)
        {
            var p = Products.FirstOrDefault(x => x.Id == item.ProductId);
            if (p != null)
            {
                p.StockQuantity -= item.Quantity;
            }
        }

        var order = new Order
        {
            Id = Orders.Count + 1,
            CustomerName = customerName,
            CustomerPhone = phone,
            Items = items
        };

        Orders.Add(order);
        return order;
    }
}
