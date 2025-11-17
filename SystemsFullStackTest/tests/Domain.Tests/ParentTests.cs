using Domain.Entities;
using FluentAssertions;

namespace Domain.Tests;

public class ParentTests
{
    [Test]
    public void DebitWallet_WithValidAmount_ShouldDecreaseBalance()
    {
        var parent = new Parent
        {
            Id = Guid.NewGuid(),
            WalletBalance = 100.00m
        };

        parent.DebitWallet(50.00m);

        parent.WalletBalance.Should().Be(50.00m);
    }

    [Test]
    public void DebitWallet_WithInsufficientBalance_ShouldThrowException()
    {
        var parent = new Parent
        {
            Id = Guid.NewGuid(),
            WalletBalance = 50.00m
        };

        var action = () => parent.DebitWallet(100.00m);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Insufficient wallet balance");
    }

    [Test]
    public void DebitWallet_WithZeroAmount_ShouldThrowException()
    {
        var parent = new Parent
        {
            Id = Guid.NewGuid(),
            WalletBalance = 100.00m
        };

        var action = () => parent.DebitWallet(0m);

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void DebitWallet_WithNegativeAmount_ShouldThrowException()
    {
        var parent = new Parent
        {
            Id = Guid.NewGuid(),
            WalletBalance = 100.00m
        };

        var action = () => parent.DebitWallet(-10m);

        action.Should().Throw<ArgumentException>();
    }
}

