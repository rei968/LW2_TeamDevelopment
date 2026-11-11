using FluentAssertions;
using Shop.Shared.Models;
using Shop.Shared.Services;
using Xunit;

namespace Shop.Tests;

public class ShopTests
{
    [Fact]
    public void OrderItem_TotalPrice_ShouldBeCalculatedCorrectly()
    {
        // Arrange
        var item = new OrderItem
        {
            ProductId = 1,
            ProductName = "Тестовий товар",
            UnitPrice = 1000m,
            Quantity = 3
        };

        // Act & Assert
        item.TotalPrice.Should().Be(3000m);
    }

    [Fact]
    public void CreateOrder_ShouldReduceProductStock()
    {
        // Arrange
        var service = new ShopService();
        var product = service.Products.First();
        int initialStock = product.StockQuantity;

        var items = new List<OrderItem>
        {
            new() { ProductId = product.Id, ProductName = product.Name, UnitPrice = product.Price, Quantity = 2 }
        };

        // Act
        service.CreateOrder("Іван", "+380501234567", items);

        // Assert
        product.StockQuantity.Should().Be(initialStock - 2);
    }

    // TODO (Задача Іллі): Додати тест перевірки статусу замовлення за замовчуванням
    // TODO (Задача Іллі): Додати тест розрахунку суми замовлення з кількома товарами
    // TODO (Задача Іллі): Додати тест блокування замовлення, коли залишок 0
}
