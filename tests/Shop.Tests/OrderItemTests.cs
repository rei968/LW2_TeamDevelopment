using Shop.Shared.DTOs.Orders;

namespace Shop.Tests;

public class OrderItemTests
{
    [Fact]
    public void TotalPrice_ShouldCalculateCorrectly()
    {
        // Arrange
        var item = new OrderItemDto
        {
            ProductId = 1,
            ProductName = "Test Product",
            UnitPrice = 250.50m,
            Quantity = 3
        };

        // Act
        var totalPrice = item.TotalPrice;

        // Assert
        Assert.Equal(751.50m, totalPrice);
    }
}
