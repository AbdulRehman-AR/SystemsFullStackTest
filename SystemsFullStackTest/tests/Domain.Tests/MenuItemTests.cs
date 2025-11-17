using Domain.Entities;
using FluentAssertions;

namespace Domain.Tests;

public class MenuItemTests
{
    [Test]
    public void HasAllergen_WithMatchingAllergen_ShouldReturnTrue()
    {
        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            AllergenTags = new List<string> { "nuts", "dairy" }
        };

        menuItem.HasAllergen("nuts").Should().BeTrue();
        menuItem.HasAllergen("NUTS").Should().BeTrue();
    }

    [Test]
    public void HasAllergen_WithNonMatchingAllergen_ShouldReturnFalse()
    {
        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            AllergenTags = new List<string> { "nuts", "dairy" }
        };

        menuItem.HasAllergen("gluten").Should().BeFalse();
    }

    [Test]
    public void IsInStock_WithUnlimitedStock_ShouldReturnTrue()
    {
        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            DailyStockCount = null
        };

        menuItem.IsInStock(100).Should().BeTrue();
    }

    [Test]
    public void IsInStock_WithSufficientStock_ShouldReturnTrue()
    {
        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            DailyStockCount = 50
        };

        menuItem.IsInStock(30).Should().BeTrue();
    }

    [Test]
    public void IsInStock_WithInsufficientStock_ShouldReturnFalse()
    {
        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            DailyStockCount = 10
        };

        menuItem.IsInStock(20).Should().BeFalse();
    }

    [Test]
    public void DecrementStock_WithValidQuantity_ShouldDecreaseStock()
    {
        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            DailyStockCount = 50
        };

        menuItem.DecrementStock(20);

        menuItem.DailyStockCount.Should().Be(30);
    }

    [Test]
    public void DecrementStock_WithInsufficientStock_ShouldThrowException()
    {
        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            DailyStockCount = 10
        };

        var action = () => menuItem.DecrementStock(20);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Insufficient stock");
    }

    [Test]
    public void DecrementStock_WithNullStock_ShouldNotThrow()
    {
        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            DailyStockCount = null
        };

        var action = () => menuItem.DecrementStock(10);

        action.Should().NotThrow();
        menuItem.DailyStockCount.Should().BeNull();
    }
}

