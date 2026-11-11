using System.Text.Json;
using Shop.Shared.DTOs;
using Shop.Shared.DTOs.Auth;
using Shop.Shared.DTOs.Categories;
using Shop.Shared.DTOs.Products;
using Shop.Shared.Enums;

namespace Shop.Shared.Services;

public class ShopDataService
{
    private static ShopDataService? _instance;
    public static ShopDataService Instance => _instance ??= new ShopDataService();

    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public List<CategoryDto> Categories { get; private set; } = new();
    public List<ProductDto> Products { get; private set; } = new();
    public List<OrderDto> Orders { get; private set; } = new();
    public List<UserDto> Users { get; private set; } = new();

    public ShopDataService(string? dataFilePath = null)
    {
        _filePath = dataFilePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shop_data.json");
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        LoadData();
    }

    public void LoadData()
    {
        if (File.Exists(_filePath))
        {
            try
            {
                var json = File.ReadAllText(_filePath);
                var data = JsonSerializer.Deserialize<ShopDataStore>(json, _jsonOptions);
                if (data != null)
                {
                    Categories = data.Categories ?? new();
                    Products = data.Products ?? new();
                    Orders = data.Orders ?? new();
                    Users = data.Users ?? new();
                    return;
                }
            }
            catch
            {
                // Fallback to initial seed if reading corrupted
            }
        }

        SeedInitialData();
        SaveData();
    }

    public void SaveData()
    {
        try
        {
            var data = new ShopDataStore
            {
                Categories = Categories,
                Products = Products,
                Orders = Orders,
                Users = Users
            };
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }
        catch
        {
            // Ignore write errors in test sandbox
        }
    }

    private void SeedInitialData()
    {
        Categories = new List<CategoryDto>
        {
            new() { Id = 1, Name = "Ноутбуки", Description = "Ноутбуки для роботи, навчання та геймінгу", ProductsCount = 3 },
            new() { Id = 2, Name = "Смартфони", Description = "Флагмани та бюджетні моделі", ProductsCount = 3 },
            new() { Id = 3, Name = "Аксесуари", Description = "Навушники, зарядні пристрої, чохли", ProductsCount = 2 }
        };

        Products = new List<ProductDto>
        {
            new() { Id = 1, Name = "MacBook Air M2", Description = "13.6-дюймовий дисплей Liquid Retina, 8 ГБ, 256 ГБ SSD", Price = 42999m, StockQuantity = 10, Brand = "Apple", CategoryId = 1, CategoryName = "Ноутбуки", AverageRating = 4.8, ReviewsCount = 5, CreatedAt = DateTime.Now.AddDays(-10) },
            new() { Id = 2, Name = "ASUS ROG Zephyrus G14", Description = "Ігровий ноутбук AMD Ryzen 9, RTX 4060, 16 ГБ ОЗП", Price = 58999m, StockQuantity = 5, Brand = "ASUS", CategoryId = 1, CategoryName = "Ноутбуки", AverageRating = 4.9, ReviewsCount = 8, CreatedAt = DateTime.Now.AddDays(-8) },
            new() { Id = 3, Name = "Lenovo ThinkPad E16", Description = "Надійний ноутбук для бізнесу, Intel Core i5, 16 ГБ ОЗП", Price = 28499m, StockQuantity = 12, Brand = "Lenovo", CategoryId = 1, CategoryName = "Ноутбуки", AverageRating = 4.5, ReviewsCount = 3, CreatedAt = DateTime.Now.AddDays(-6) },
            new() { Id = 4, Name = "iPhone 15 Pro", Description = "Титан, Dynamic Island, чип A17 Pro, 128 ГБ", Price = 44999m, StockQuantity = 15, Brand = "Apple", CategoryId = 2, CategoryName = "Смартфони", AverageRating = 4.9, ReviewsCount = 12, CreatedAt = DateTime.Now.AddDays(-5) },
            new() { Id = 5, Name = "Samsung Galaxy S24 Ultra", Description = "Galaxy AI, 200 Мп камера, S Pen, 256 ГБ", Price = 49999m, StockQuantity = 8, Brand = "Samsung", CategoryId = 2, CategoryName = "Смартфони", AverageRating = 4.7, ReviewsCount = 7, CreatedAt = DateTime.Now.AddDays(-4) },
            new() { Id = 6, Name = "Google Pixel 8", Description = "Чистий Android, неймовірні фото, чип Tensor G3", Price = 26999m, StockQuantity = 7, Brand = "Google", CategoryId = 2, CategoryName = "Смартфони", AverageRating = 4.6, ReviewsCount = 4, CreatedAt = DateTime.Now.AddDays(-3) },
            new() { Id = 7, Name = "AirPods Pro 2", Description = "Активне шумозаглушення, USB-C, просторове аудіо", Price = 9999m, StockQuantity = 20, Brand = "Apple", CategoryId = 3, CategoryName = "Аксесуари", AverageRating = 4.8, ReviewsCount = 15, CreatedAt = DateTime.Now.AddDays(-2) },
            new() { Id = 8, Name = "Samsung Galaxy Buds2 Pro", Description = "Бездротові навушники з Hi-Fi 24-біт звуком", Price = 5999m, StockQuantity = 14, Brand = "Samsung", CategoryId = 3, CategoryName = "Аксесуари", AverageRating = 4.4, ReviewsCount = 6, CreatedAt = DateTime.Now.AddDays(-1) }
        };

        Users = new List<UserDto>
        {
            new() { Id = "1", Email = "admin@shop.com", FullName = "Головний Менеджер", Role = UserRole.Admin },
            new() { Id = "2", Email = "customer@shop.com", FullName = "Іван Покупець", Role = UserRole.Customer }
        };

        Orders = new List<OrderDto>
        {
            new()
            {
                Id = 1,
                OrderNumber = "ORD-1001",
                CustomerEmail = "customer@shop.com",
                CustomerName = "Іван Покупець",
                CustomerPhone = "+380501234567",
                ShippingAddress = "м. Київ, вул. Хрещатик, 1",
                Status = OrderStatus.Processing,
                CreatedAt = DateTime.Now.AddDays(-2),
                TotalAmount = 52998m,
                Items = new List<OrderItemDto>
                {
                    new() { Id = 1, ProductId = 1, ProductName = "MacBook Air M2", UnitPrice = 42999m, Quantity = 1 },
                    new() { Id = 2, ProductId = 7, ProductName = "AirPods Pro 2", UnitPrice = 9999m, Quantity = 1 }
                }
            }
        };
    }

    // --- Робота з каталогом ---
    public List<ProductDto> GetProducts(string? search = null, int? categoryId = null, string? brand = null, decimal? maxPrice = null)
    {
        var query = Products.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s) || p.Brand.ToLower().Contains(s) || p.Description.ToLower().Contains(s));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(brand))
        {
            query = query.Where(p => p.Brand.Equals(brand, StringComparison.OrdinalIgnoreCase));
        }

        if (maxPrice.HasValue && maxPrice.Value > 0)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        return query.ToList();
    }

    public ProductDto? GetProductById(int id) => Products.FirstOrDefault(p => p.Id == id);

    public ProductDto AddProduct(CreateProductDto dto)
    {
        var category = Categories.FirstOrDefault(c => c.Id == dto.CategoryId);
        var newId = Products.Count > 0 ? Products.Max(p => p.Id) + 1 : 1;

        var product = new ProductDto
        {
            Id = newId,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            Brand = dto.Brand,
            CategoryId = dto.CategoryId,
            CategoryName = category?.Name ?? "Різне",
            CreatedAt = DateTime.Now
        };

        Products.Add(product);
        if (category != null) category.ProductsCount++;
        SaveData();
        return product;
    }

    public bool UpdateProduct(int id, UpdateProductDto dto)
    {
        var product = Products.FirstOrDefault(p => p.Id == id);
        if (product == null) return false;

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.Brand = dto.Brand;

        if (product.CategoryId != dto.CategoryId)
        {
            var oldCat = Categories.FirstOrDefault(c => c.Id == product.CategoryId);
            if (oldCat != null) oldCat.ProductsCount = Math.Max(0, oldCat.ProductsCount - 1);

            var newCat = Categories.FirstOrDefault(c => c.Id == dto.CategoryId);
            if (newCat != null) newCat.ProductsCount++;

            product.CategoryId = dto.CategoryId;
            product.CategoryName = newCat?.Name ?? "Різне";
        }

        SaveData();
        return true;
    }

    public bool UpdateStock(int id, int newQuantity)
    {
        var product = Products.FirstOrDefault(p => p.Id == id);
        if (product == null || newQuantity < 0) return false;

        product.StockQuantity = newQuantity;
        SaveData();
        return true;
    }

    public bool DeleteProduct(int id)
    {
        var product = Products.FirstOrDefault(p => p.Id == id);
        if (product == null) return false;

        var category = Categories.FirstOrDefault(c => c.Id == product.CategoryId);
        if (category != null) category.ProductsCount = Math.Max(0, category.ProductsCount - 1);

        Products.Remove(product);
        SaveData();
        return true;
    }

    // --- Робота з замовленнями ---
    public OrderDto CreateOrder(CreateOrderDto dto, string userEmail)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ArgumentException("Кошик замовлення не може бути порожнім.");

        var orderItems = new List<OrderItemDto>();
        decimal total = 0;

        foreach (var item in dto.Items)
        {
            var product = Products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Товар з ID {item.ProductId} не знайдено.");

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Недостатньо залишку товару '{product.Name}'. Доступно: {product.StockQuantity}, замовлено: {item.Quantity}.");

            // Списання зі складу
            product.StockQuantity -= item.Quantity;

            var orderItem = new OrderItemDto
            {
                Id = orderItems.Count + 1,
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = item.Quantity
            };

            orderItems.Add(orderItem);
            total += orderItem.TotalPrice;
        }

        var newOrderId = Orders.Count > 0 ? Orders.Max(o => o.Id) + 1 : 1;
        var order = new OrderDto
        {
            Id = newOrderId,
            OrderNumber = $"ORD-{1000 + newOrderId}",
            CustomerEmail = userEmail,
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone,
            ShippingAddress = dto.ShippingAddress,
            Status = OrderStatus.New,
            CreatedAt = DateTime.Now,
            TotalAmount = total,
            Items = orderItems
        };

        Orders.Insert(0, order);
        SaveData();
        return order;
    }

    public bool UpdateOrderStatus(int orderId, OrderStatus newStatus)
    {
        var order = Orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null) return false;

        // Якщо замовлення скасовується — повертаємо залишки на склад
        if (newStatus == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
        {
            foreach (var item in order.Items)
            {
                var product = Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                }
            }
        }

        order.Status = newStatus;
        SaveData();
        return true;
    }

    // --- Авторизація ---
    public (bool Success, UserDto? User, string Error) Login(string email, string password)
    {
        var user = Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (user == null)
        {
            return (false, null, "Користувача з такою поштою не знайдено.");
        }

        // Для спрощення: пароль admin123 для адміна, customer123 для клієнта
        if ((user.Role == UserRole.Admin && password != "admin123") ||
            (user.Role == UserRole.Customer && password != "customer123"))
        {
            return (false, null, "Невірний пароль.");
        }

        return (true, user, string.Empty);
    }
}

public class ShopDataStore
{
    public List<CategoryDto> Categories { get; set; } = new();
    public List<ProductDto> Products { get; set; } = new();
    public List<OrderDto> Orders { get; set; } = new();
    public List<UserDto> Users { get; set; } = new();
}
