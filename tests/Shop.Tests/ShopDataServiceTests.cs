using FluentAssertions;
using Shop.Shared.DTOs;
using Shop.Shared.DTOs.Products;
using Shop.Shared.Enums;
using Shop.Shared.Services;
using Xunit;

namespace Shop.Tests;

public class ShopDataServiceTests
{
    private ShopDataService CreateService()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_shop_{Guid.NewGuid():N}.json");
        return new ShopDataService(tempFile);
    }

    [Fact]
    public void GetProducts_WithMaxPriceFilter_ReturnsOnlyAffordableItems()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = service.GetProducts(maxPrice: 30000m);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(p => p.Price <= 30000m);
    }

    [Fact]
    public void CreateOrder_WithValidStock_DeductsStockAndCreatesOrder()
    {
        // Arrange
        var service = CreateService();
        var product = service.Products.First(p => p.StockQuantity >= 3);
        int initialStock = product.StockQuantity;

        var orderDto = new CreateOrderDto
        {
            CustomerName = "Тестовий Покупець",
            CustomerPhone = "+380501112233",
            ShippingAddress = "вул. Тестова, 1",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 2 }
            }
        };

        // Act
        var order = service.CreateOrder(orderDto, "test@shop.com");

        // Assert
        order.Should().NotBeNull();
        order.Status.Should().Be(OrderStatus.New);
        product.StockQuantity.Should().Be(initialStock - 2);
        order.TotalAmount.Should().Be(product.Price * 2);
    }

    [Fact]
    public void CreateOrder_WithInsufficientStock_ThrowsException()
    {
        // Arrange
        var service = CreateService();
        var product = service.Products.First();

        var orderDto = new CreateOrderDto
        {
            CustomerName = "Покупець",
            CustomerPhone = "+380501112233",
            ShippingAddress = "вул. Тестова, 1",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = product.StockQuantity + 100 }
            }
        };

        // Act
        var action = () => service.CreateOrder(orderDto, "test@shop.com");

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Недостатньо залишку*");
    }

    [Fact]
    public void UpdateOrderStatus_ToCancelled_RestoresStockQuantity()
    {
        // Arrange
        var service = CreateService();
        var product = service.Products.First(p => p.StockQuantity >= 2);
        int stockBeforeOrder = product.StockQuantity;

        var orderDto = new CreateOrderDto
        {
            CustomerName = "Покупець",
            CustomerPhone = "+380501112233",
            ShippingAddress = "вул. Тестова, 1",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 2 }
            }
        };

        var order = service.CreateOrder(orderDto, "test@shop.com");
        product.StockQuantity.Should().Be(stockBeforeOrder - 2);

        // Act
        service.UpdateOrderStatus(order.Id, OrderStatus.Cancelled);

        // Assert
        product.StockQuantity.Should().Be(stockBeforeOrder);
    }
}
