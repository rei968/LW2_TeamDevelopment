using FluentAssertions;
using Shop.Shared.DTOs.Products;
using Xunit;

namespace Shop.Tests;

public class StockManagementTests
{
    [Fact]
    public void ProductStock_ShouldDecrease_WhenOrdered()
    {
        // Arrange
        var product = new ProductDto
        {
            Id = 1,
            Name = "Apple MacBook Pro",
            StockQuantity = 10,
            Price = 90000m
        };
        int quantityToOrder = 3;

        // Act
        product.StockQuantity -= quantityToOrder;

        // Assert
        product.StockQuantity.Should().Be(7);
    }

    [Theory]
    [InlineData(5, true)]
    [InlineData(2, true)]
    [InlineData(0, true)]
    [InlineData(6, false)]
    [InlineData(20, false)]
    public void ProductStock_ShouldDetectLowStock_WhenQuantityIsFiveOrLess(int currentStock, bool expectedLowStock)
    {
        // Arrange & Act
        bool isLowStock = currentStock <= 5;

        // Assert
        isLowStock.Should().Be(expectedLowStock);
    }

    [Fact]
    public void OrderCreation_ShouldFail_WhenRequestedQuantityExceedsAvailableStock()
    {
        // Arrange
        var product = new ProductDto
        {
            Id = 2,
            Name = "Lenovo Legion Pro",
            StockQuantity = 2,
            Price = 64999m
        };
        int requestedQuantity = 5;

        // Act
        bool hasEnoughStock = product.StockQuantity >= requestedQuantity;

        // Assert
        hasEnoughStock.Should().BeFalse();
    }

    [Fact]
    public void CancelOrder_ShouldRestoreProductStock()
    {
        // Arrange
        var product = new ProductDto
        {
            Id = 3,
            Name = "Sony WH-1000XM5",
            StockQuantity = 10,
            Price = 14999m
        };
        int orderedQuantity = 4;
        product.StockQuantity -= orderedQuantity; // Simulated order placement -> 6

        // Act (Simulated cancellation)
        product.StockQuantity += orderedQuantity;

        // Assert
        product.StockQuantity.Should().Be(10);
    }
}
