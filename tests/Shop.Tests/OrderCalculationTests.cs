using FluentAssertions;
using Shop.Shared.DTOs;
using Xunit;

namespace Shop.Tests;

public class OrderCalculationTests
{
    [Fact]
    public void OrderItem_TotalPrice_ShouldBeUnitPriceTimesQuantity()
    {
        // Arrange
        var item = new OrderItemDto
        {
            ProductId = 1,
            ProductName = "AirPods Pro",
            UnitPrice = 9999m,
            Quantity = 3
        };

        // Act & Assert
        item.TotalPrice.Should().Be(29997m);
    }

    [Fact]
    public void Order_TotalAmount_ShouldSumAllItemTotals()
    {
        // Arrange
        var items = new List<OrderItemDto>
        {
            new() { ProductId = 1, ProductName = "Product 1", UnitPrice = 1000m, Quantity = 2 }, // 2000
            new() { ProductId = 2, ProductName = "Product 2", UnitPrice = 500m, Quantity = 3 },  // 1500
            new() { ProductId = 3, ProductName = "Product 3", UnitPrice = 250m, Quantity = 4 }   // 1000
        };

        // Act
        decimal total = items.Sum(i => i.TotalPrice);

        // Assert
        total.Should().Be(4500m);
    }
}
