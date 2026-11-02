using FluentAssertions;
using Shop.Shared.DTOs;
using Shop.Shared.Enums;
using Xunit;

namespace Shop.Tests;

public class OrderStatusTransitionTests
{
    private bool IsValidTransition(OrderStatus current, OrderStatus next)
    {
        if (current == OrderStatus.Completed && next == OrderStatus.New)
            return false;

        if (current == OrderStatus.Cancelled && next != OrderStatus.Cancelled)
            return false;

        if (current == OrderStatus.New && (next == OrderStatus.Processing || next == OrderStatus.Cancelled))
            return true;

        if (current == OrderStatus.Processing && (next == OrderStatus.Completed || next == OrderStatus.Cancelled))
            return true;

        if (current == next)
            return true;

        return false;
    }

    [Fact]
    public void OrderStatus_ShouldAllowTransition_FromNewToProcessing()
    {
        // Arrange
        var current = OrderStatus.New;
        var next = OrderStatus.Processing;

        // Act & Assert
        IsValidTransition(current, next).Should().BeTrue();
    }

    [Fact]
    public void OrderStatus_ShouldAllowTransition_FromProcessingToCompleted()
    {
        // Arrange
        var current = OrderStatus.Processing;
        var next = OrderStatus.Completed;

        // Act & Assert
        IsValidTransition(current, next).Should().BeTrue();
    }

    [Fact]
    public void OrderStatus_ShouldRejectTransition_FromCompletedToNew()
    {
        // Arrange
        var current = OrderStatus.Completed;
        var next = OrderStatus.New;

        // Act & Assert
        IsValidTransition(current, next).Should().BeFalse();
    }

    [Fact]
    public void OrderStatus_ShouldRejectTransition_FromCancelledToProcessing()
    {
        // Arrange
        var current = OrderStatus.Cancelled;
        var next = OrderStatus.Processing;

        // Act & Assert
        IsValidTransition(current, next).Should().BeFalse();
    }
}
