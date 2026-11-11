using Shop.Shared.DTOs;
using Shop.Shared.DTOs.Auth;
using Shop.Shared.DTOs.Categories;
using Shop.Shared.DTOs.Products;
using Shop.Shared.Enums;
using Shop.Shared.Services;

namespace Shop.WinForms.Services;

public class ApiClient
{
    private static ApiClient? _instance;
    public static ApiClient Instance => _instance ??= new ApiClient();

    private readonly ShopDataService _dataService = ShopDataService.Instance;

    public string BaseUrl { get; private set; } = "Local / JSON Storage";
    public string? Token { get; private set; }
    public string? CurrentEmail { get; private set; }
    public string? CurrentFullName { get; private set; }
    public string? CurrentRole { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentEmail);
    public bool IsAdmin => CurrentRole == UserRole.Admin;

    public ApiClient()
    {
    }

    public void SetBaseUrl(string url)
    {
        BaseUrl = url;
    }

    public void SetToken(string token, string email, string fullName, string role)
    {
        Token = token;
        CurrentEmail = email;
        CurrentFullName = fullName;
        CurrentRole = role;
    }

    public void ClearToken()
    {
        Token = null;
        CurrentEmail = null;
        CurrentFullName = null;
        CurrentRole = null;
    }

    public void Logout()
    {
        ClearToken();
    }

    // --- Авторизація ---
    public Task<(bool Success, string Message)> LoginAsync(string email, string password)
    {
        var result = _dataService.Login(email, password);
        if (result.Success && result.User != null)
        {
            SetToken("local-token-xyz", result.User.Email, result.User.FullName, result.User.Role);
            return Task.FromResult((true, "Успішний вхід"));
        }

        return Task.FromResult((false, result.Error));
    }

    public Task<(bool Success, string Message)> RegisterAsync(string email, string password, string fullName)
    {
        var existing = _dataService.Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            return Task.FromResult((false, "Користувач з таким Email вже існує"));
        }

        var newUser = new UserDto
        {
            Id = (_dataService.Users.Count + 1).ToString(),
            Email = email,
            FullName = fullName,
            Role = UserRole.Customer
        };

        _dataService.Users.Add(newUser);
        _dataService.SaveData();

        SetToken("local-token-xyz", newUser.Email, newUser.FullName, newUser.Role);
        return Task.FromResult((true, "Реєстрація успішна"));
    }

    // --- Категорії ---
    public Task<List<CategoryDto>> GetCategoriesAsync()
    {
        return Task.FromResult(_dataService.Categories.ToList());
    }

    public Task<(bool Success, string Message)> CreateCategoryAsync(CategoryDto dto)
    {
        var newId = _dataService.Categories.Count > 0 ? _dataService.Categories.Max(c => c.Id) + 1 : 1;
        dto.Id = newId;
        _dataService.Categories.Add(dto);
        _dataService.SaveData();
        return Task.FromResult((true, "Категорію створено"));
    }

    public Task<(bool Success, string Message)> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var newId = _dataService.Categories.Count > 0 ? _dataService.Categories.Max(c => c.Id) + 1 : 1;
        var cat = new CategoryDto
        {
            Id = newId,
            Name = dto.Name,
            Description = dto.Description
        };
        _dataService.Categories.Add(cat);
        _dataService.SaveData();
        return Task.FromResult((true, "Категорію створено"));
    }

    public Task<(bool Success, string Message)> UpdateCategoryAsync(int id, CategoryDto dto)
    {
        var cat = _dataService.Categories.FirstOrDefault(c => c.Id == id);
        if (cat == null) return Task.FromResult((false, "Категорію не знайдено"));

        cat.Name = dto.Name;
        cat.Description = dto.Description;
        _dataService.SaveData();
        return Task.FromResult((true, "Категорію оновлено"));
    }

    public Task<(bool Success, string Message)> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        var cat = _dataService.Categories.FirstOrDefault(c => c.Id == id);
        if (cat == null) return Task.FromResult((false, "Категорію не знайдено"));

        cat.Name = dto.Name;
        cat.Description = dto.Description;
        _dataService.SaveData();
        return Task.FromResult((true, "Категорію оновлено"));
    }

    public Task<(bool Success, string Message)> DeleteCategoryAsync(int id)
    {
        var cat = _dataService.Categories.FirstOrDefault(c => c.Id == id);
        if (cat == null) return Task.FromResult((false, "Категорію не знайдено"));

        _dataService.Categories.Remove(cat);
        _dataService.SaveData();
        return Task.FromResult((true, "Категорію видалено"));
    }

    // --- Товари ---
    public Task<List<ProductDto>> GetProductsAsync(int? categoryId = null, string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, string? search = null, string? sortBy = null)
    {
        var list = _dataService.GetProducts(search, categoryId, brand, maxPrice);
        if (minPrice.HasValue && minPrice.Value > 0)
        {
            list = list.Where(p => p.Price >= minPrice.Value).ToList();
        }

        if (sortBy == "price_asc") list = list.OrderBy(p => p.Price).ToList();
        else if (sortBy == "price_desc") list = list.OrderByDescending(p => p.Price).ToList();
        else if (sortBy == "name") list = list.OrderBy(p => p.Name).ToList();

        return Task.FromResult(list);
    }

    public Task<ProductDto?> GetProductByIdAsync(int id)
    {
        return Task.FromResult(_dataService.GetProductById(id));
    }

    public Task<(bool Success, string Message)> CreateProductAsync(CreateProductDto dto)
    {
        _dataService.AddProduct(dto);
        return Task.FromResult((true, "Товар успішно створено"));
    }

    public Task<(bool Success, string Message)> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var ok = _dataService.UpdateProduct(id, dto);
        return Task.FromResult((ok, ok ? "Товар оновлено" : "Товар не знайдено"));
    }

    public Task<(bool Success, string Message)> UpdateStockAsync(int id, int stockQuantity)
    {
        var ok = _dataService.UpdateStock(id, stockQuantity);
        return Task.FromResult((ok, ok ? "Залишок оновлено" : "Помилка оновлення"));
    }

    public Task<(bool Success, string Message)> DeleteProductAsync(int id)
    {
        var ok = _dataService.DeleteProduct(id);
        return Task.FromResult((ok, ok ? "Товар видалено" : "Товар не знайдено"));
    }

    // --- Замовлення ---
    public Task<List<OrderDto>> GetOrdersAsync(OrderStatus? status = null)
    {
        var orders = _dataService.Orders.AsEnumerable();
        if (status.HasValue)
        {
            orders = orders.Where(o => o.Status == status.Value);
        }
        return Task.FromResult(orders.ToList());
    }

    public Task<List<OrderDto>> GetMyOrdersAsync()
    {
        var myEmail = CurrentEmail ?? "customer@shop.com";
        var orders = _dataService.Orders.Where(o => o.CustomerEmail.Equals(myEmail, StringComparison.OrdinalIgnoreCase)).ToList();
        return Task.FromResult(orders);
    }

    public Task<(bool Success, string Message, OrderDto? Order)> CreateOrderAsync(CreateOrderDto dto)
    {
        try
        {
            var userEmail = CurrentEmail ?? "customer@shop.com";
            var order = _dataService.CreateOrder(dto, userEmail);
            return Task.FromResult((true, "Замовлення оформлено", (OrderDto?)order));
        }
        catch (Exception ex)
        {
            return Task.FromResult((false, ex.Message, (OrderDto?)null));
        }
    }

    public Task<(bool Success, string Message)> UpdateOrderStatusAsync(int id, OrderStatus newStatus)
    {
        var ok = _dataService.UpdateOrderStatus(id, newStatus);
        return Task.FromResult((ok, ok ? "Статус замовлення оновлено" : "Замовлення не знайдено"));
    }
}
