using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=Database/ordering.db";
var dbPath = defaultConnection.Replace("Data Source=", "");
if (!Path.IsPathRooted(dbPath))
{
    var basePath = builder.Environment.ContentRootPath;
    dbPath = Path.Combine(basePath, dbPath);
}
var dbDirectory = Path.GetDirectoryName(dbPath);
if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
{
    Directory.CreateDirectory(dbDirectory);
}
var connectionString = $"Data Source={dbPath}";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IParentRepository, ParentRepository>();
builder.Services.AddScoped<IRepository<Domain.Entities.Student>, StudentRepository>();
builder.Services.AddScoped<IRepository<Domain.Entities.Canteen>, CanteenRepository>();
builder.Services.AddScoped<IRepository<Domain.Entities.MenuItem>, Repository<Domain.Entities.MenuItem>>();

builder.Services.AddScoped<IDateTimeProvider, DateTimeProvider>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Application.Commands.CreateOrderCommand).Assembly));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }
    else
    {
        dbContext.Database.EnsureCreated();
    }

    // Seed initial data
    await DataSeeder.SeedData(scope.ServiceProvider);
}

await app.RunAsync();

public partial class Program { }
