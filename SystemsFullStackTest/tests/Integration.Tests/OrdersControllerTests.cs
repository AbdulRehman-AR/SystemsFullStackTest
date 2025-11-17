using System.Net;
using System.Net.Http.Json;
using Application.DTOs;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.Tests;

public class OrdersControllerTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private ApplicationDbContext _context = null!;
    private IServiceScope _scope = null!;
    private string _databaseName = string.Empty;

    [SetUp]
    public void SetUp()
    {
        _databaseName = "TestDb_" + Guid.NewGuid();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(_databaseName);
                    });
                });
            });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _context.Database.EnsureCreated();
        SeedTestData();
    }

    private void SeedTestData()
    {
        var parent = new Parent
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "parent@test.com",
            Name = "Test Parent",
            WalletBalance = 1000.00m
        };

        var student = new Student
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Test Student",
            ParentId = parent.Id,
            Parent = parent,
            Allergen = null
        };

        var canteen = new Canteen
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Test Canteen",
            Schedules = new List<CanteenSchedule>
            {
                new CanteenSchedule
                {
                    Id = Guid.NewGuid(),
                    CanteenId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    DayOfWeek = DateTime.Today.DayOfWeek,
                    CutoffTime = new TimeSpan(23, 59, 59)
                }
            }
        };

        var menuItem = new MenuItem
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Name = "Test Item",
            Price = 10.00m,
            CanteenId = canteen.Id,
            Canteen = canteen,
            DailyStockCount = 100,
            AllergenTags = new List<string>()
        };

        _context.Parents.Add(parent);
        _context.Students.Add(student);
        _context.Canteens.Add(canteen);
        _context.MenuItems.Add(menuItem);
        _context.SaveChanges();
    }

    [Test]
    public async Task CreateOrder_WithValidData_ShouldReturnCreated()
    {
        var orderDto = new CreateOrderDto
        {
            ParentId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            StudentId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            CanteenId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            FulfilmentDate = DateTime.Today,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    MenuItemId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Quantity = 2
                }
            }
        };

        var response = await PostOrderAsync(orderDto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();
        order!.TotalAmount.Should().Be(20.00m);
        order.Status.Should().Be("Confirmed");
    }

    [Test]
    public async Task CreateOrder_WithInsufficientBalance_ShouldReturnBadRequest()
    {
        var parent = _context.Parents.First();
        parent.WalletBalance = 5.00m;
        _context.SaveChanges();

        var orderDto = new CreateOrderDto
        {
            ParentId = parent.Id,
            StudentId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            CanteenId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            FulfilmentDate = DateTime.Today,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    MenuItemId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Quantity = 10
                }
            }
        };

        var response = await PostOrderAsync(orderDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateOrder_WithIdempotencyKey_ShouldReturnSameOrder()
    {
        var orderDto = new CreateOrderDto
        {
            ParentId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            StudentId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            CanteenId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            FulfilmentDate = DateTime.Today,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    MenuItemId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Quantity = 1
                }
            }
        };

        var idempotencyKey = Guid.NewGuid().ToString();

        _client.DefaultRequestHeaders.Remove("Idempotency-Key");
        _client.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKey);

        var response1 = await PostOrderAsync(orderDto);
        var order1 = await response1.Content.ReadFromJsonAsync<OrderDto>();

        var response2 = await PostOrderAsync(orderDto);
        var order2 = await response2.Content.ReadFromJsonAsync<OrderDto>();

        order1!.Id.Should().Be(order2!.Id);

        _client.DefaultRequestHeaders.Remove("Idempotency-Key");
    }

    [Test]
    public async Task GetOrder_WithValidId_ShouldReturnOrder()
    {
        var orderDto = new CreateOrderDto
        {
            ParentId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            StudentId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            CanteenId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            FulfilmentDate = DateTime.Today,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    MenuItemId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Quantity = 1
                }
            }
        };

        var createResponse = await PostOrderAsync(orderDto);
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        var getResponse = await _client.GetAsync($"/api/orders/{createdOrder!.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await getResponse.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();
        order!.Id.Should().Be(createdOrder.Id);
    }

    [Test]
    public async Task GetOrder_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _scope.Dispose();
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<HttpResponseMessage> PostOrderAsync(CreateOrderDto dto)
    {
        var response = await _client.PostAsJsonAsync("/api/orders", dto);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            TestContext.Out.WriteLine($"Response body: {content}");
        }

        return response;
    }
}

