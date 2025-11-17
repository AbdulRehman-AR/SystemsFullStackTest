using Domain.Entities;
using FluentAssertions;

namespace Domain.Tests;

public class OrderTests
{
    [Test]
    public void Confirm_WithPlacedStatus_ShouldChangeToConfirmed()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Status = OrderStatus.Placed
        };

        order.Confirm();

        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Test]
    public void Confirm_WithNonPlacedStatus_ShouldThrowException()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Status = OrderStatus.Confirmed
        };

        var action = () => order.Confirm();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only placed orders can be confirmed");
    }

    [Test]
    public void CalculateTotal_WithMultipleItems_ShouldReturnCorrectTotal()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Items = new List<OrderItem>
            {
                new OrderItem { Quantity = 2, UnitPrice = 10.00m },
                new OrderItem { Quantity = 3, UnitPrice = 5.00m }
            }
        };

        var total = order.CalculateTotal();

        total.Should().Be(35.00m);
    }

    [Test]
    public void CalculateTotal_WithNoItems_ShouldReturnZero()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Items = new List<OrderItem>()
        };

        var total = order.CalculateTotal();

        total.Should().Be(0m);
    }
}

