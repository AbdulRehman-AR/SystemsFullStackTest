using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();

        try
        {
            // Only apply migrations if using a relational database
            if (context.Database.IsRelational())
            {
                await context.Database.MigrateAsync();
            }
            else
            {
                // For in-memory database, just ensure it's created
                await context.Database.EnsureCreatedAsync();
            }

            // Only seed if the database is empty
            if (await context.Parents.AnyAsync())
            {
                return; // DB has been seeded
            }

        // Create Parent
        var parent = new Parent
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john.doe@example.com",
            WalletBalance = 1000.00m
        };

        // Create Student linked to the Parent
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Jane Doe",
            ParentId = parent.Id,
            Allergen = "Peanuts"
        };

        // Create Canteen
        var canteen = new Canteen
        {
            Id = Guid.NewGuid(),
            Name = "Main School Canteen"
        };

        // Add Canteen Schedule (Monday to Friday, cutoff at 10:00 AM)
        var schedule = new List<CanteenSchedule>
        {
            new() { Id = Guid.NewGuid(), CanteenId = canteen.Id, DayOfWeek = DayOfWeek.Monday, CutoffTime = new TimeSpan(10, 0, 0) },
            new() { Id = Guid.NewGuid(), CanteenId = canteen.Id, DayOfWeek = DayOfWeek.Tuesday, CutoffTime = new TimeSpan(10, 0, 0) },
            new() { Id = Guid.NewGuid(), CanteenId = canteen.Id, DayOfWeek = DayOfWeek.Wednesday, CutoffTime = new TimeSpan(10, 0, 0) },
            new() { Id = Guid.NewGuid(), CanteenId = canteen.Id, DayOfWeek = DayOfWeek.Thursday, CutoffTime = new TimeSpan(10, 0, 0) },
            new() { Id = Guid.NewGuid(), CanteenId = canteen.Id, DayOfWeek = DayOfWeek.Friday, CutoffTime = new TimeSpan(10, 0, 0) }
        };

        // Create Menu Items
        var menuItems = new List<MenuItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Chicken Sandwich",
                Price = 5.99m,
                CanteenId = canteen.Id,
                DailyStockCount = 50,
                AllergenTags = new List<string> { "Wheat", "Dairy" }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Vegetable Pasta",
                Price = 4.99m,
                CanteenId = canteen.Id,
                DailyStockCount = 30,
                AllergenTags = new List<string> { "Gluten", "Dairy" }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Fruit Salad",
                Price = 3.49m,
                CanteenId = canteen.Id,
                DailyStockCount = null, // Unlimited
                AllergenTags = new List<string>()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Peanut Butter Sandwich",
                Price = 3.99m,
                CanteenId = canteen.Id,
                DailyStockCount = 20,
                AllergenTags = new List<string> { "Peanuts", "Wheat" }
            }
        };

        // Add all entities to the context
        await context.Parents.AddAsync(parent);
        await context.Students.AddAsync(student);
        await context.Canteens.AddAsync(canteen);
        await context.CanteenSchedules.AddRangeAsync(schedule);
        await context.MenuItems.AddRangeAsync(menuItems);

            // Save changes to the database
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error seeding database: {ex.Message}");
            throw; // Re-throw to fail the application startup if seeding fails
        }
    }
}
