using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Shop.Shared.DTOs;
using Shop.Shared.DTOs.Auth;
using Shop.Shared.DTOs.Categories;
using Shop.Shared.DTOs.Products;
using Shop.Shared.Enums;

namespace Shop.WinForms.Services;

public class ApiClient
{
    private static ApiClient? _instance;
    public static ApiClient Instance => _instance ??= new ApiClient();

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public string BaseUrl { get; private set; } = "http://localhost:5250";
    public string? Token { get; private set; }
    public string? CurrentEmail { get; private set; }
    public string? CurrentFullName { get; private set; }
    public string? CurrentRole { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);
    public bool IsAdmin => CurrentRole == UserRole.Admin;

    public ApiClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(15)
        };

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public void SetBaseUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        BaseUrl = url.TrimEnd('/');
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public void SetToken(string token, string email, string fullName, string role)
    {
        Token = token;
        CurrentEmail = email;
        CurrentFullName = fullName;
        CurrentRole = role;

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void Logout()
    {
        Token = null;
        CurrentEmail = null;
        CurrentFullName = null;
        CurrentRole = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    // --- AUTHENTICATION ---
    public async Task<(bool success, string message)> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Email = email,
                Password = password
            });

            if (response.IsSuccessStatusCode)
            {
                var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);
                if (auth != null)
                {
                    SetToken(auth.Token, auth.Email, auth.FullName, auth.Role);
                    return (true, "Успішний вхід");
                }
            }

            var err = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(err) ? "Невірні облікові дані" : err);
        }
        catch (Exception ex)
        {
            return (false, $"Помилка підключення до сервера: {ex.Message}");
        }
    }

    // --- PRODUCTS ---
    public async Task<List<ProductDto>> GetProductsAsync(
        int? categoryId = null,
        string? search = null,
        string? brand = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? sortBy = null)
    {
        var queryParams = new List<string>();

        if (categoryId.HasValue && categoryId.Value > 0)
            queryParams.Add($"categoryId={categoryId.Value}");

        if (!string.IsNullOrWhiteSpace(search))
            queryParams.Add($"search={Uri.EscapeDataString(search.Trim())}");

        if (!string.IsNullOrWhiteSpace(brand))
            queryParams.Add($"brand={Uri.EscapeDataString(brand.Trim())}");

        if (minPrice.HasValue)
            queryParams.Add($"minPrice={minPrice.Value}");

        if (maxPrice.HasValue)
            queryParams.Add($"maxPrice={maxPrice.Value}");

        if (!string.IsNullOrWhiteSpace(sortBy))
            queryParams.Add($"sortBy={sortBy}");

        var url = "/api/products";
        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>(_jsonOptions);
        return products ?? new List<ProductDto>();
    }

    public async Task<(bool success, string message)> CreateProductAsync(CreateProductDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/products", dto);
            if (response.IsSuccessStatusCode)
                return (true, "Товар успішно додано");

            var err = await response.Content.ReadAsStringAsync();
            return (false, err);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string message)> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/products/{id}", dto);
            if (response.IsSuccessStatusCode)
                return (true, "Товар успішно оновлено");

            var err = await response.Content.ReadAsStringAsync();
            return (false, err);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string message)> DeleteProductAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/products/{id}");
            if (response.IsSuccessStatusCode)
                return (true, "Товар успішно видалено");

            var err = await response.Content.ReadAsStringAsync();
            return (false, err);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string message)> UpdateStockAsync(int id, int stockQuantity)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"/api/products/{id}/stock", new UpdateStockDto
            {
                StockQuantity = stockQuantity
            });

            if (response.IsSuccessStatusCode)
                return (true, "Залишок оновлено");

            var err = await response.Content.ReadAsStringAsync();
            return (false, err);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // --- CATEGORIES ---
    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        var response = await _httpClient.GetAsync("/api/categories");
        response.EnsureSuccessStatusCode();

        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>(_jsonOptions);
        return categories ?? new List<CategoryDto>();
    }

    public async Task<(bool success, string message)> CreateCategoryAsync(CreateCategoryDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/categories", dto);
            if (response.IsSuccessStatusCode)
                return (true, "Категорію створено");

            var err = await response.Content.ReadAsStringAsync();
            return (false, err);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string message)> DeleteCategoryAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/categories/{id}");
            if (response.IsSuccessStatusCode)
                return (true, "Категорію видалено");

            var err = await response.Content.ReadAsStringAsync();
            return (false, err);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // --- ORDERS ---
    public async Task<List<OrderDto>> GetOrdersAsync(OrderStatus? status = null)
    {
        var url = "/api/orders";
        if (status.HasValue)
            url += $"?status={status.Value}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>(_jsonOptions);
        return orders ?? new List<OrderDto>();
    }

    public async Task<(bool success, string message)> UpdateOrderStatusAsync(int id, OrderStatus status)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/orders/{id}/status", new UpdateOrderStatusDto
            {
                Status = status
            });

            if (response.IsSuccessStatusCode)
                return (true, "Статус замовлення оновлено");

            var err = await response.Content.ReadAsStringAsync();
            return (false, err);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
