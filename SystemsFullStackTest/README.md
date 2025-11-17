# Ordering & Payments API

A minimal ordering and payments API built with .NET 8, implementing Clean Architecture principles with CQRS pattern.

## Architecture

### Clean Architecture Layers

- **Domain**: Core business entities, value objects, and domain interfaces. Contains no dependencies on other layers.
- **Application**: Use cases, commands, queries, DTOs, and application services. Depends only on Domain.
- **Infrastructure**: Data access (EF Core), repository implementations, and external service integrations. Depends on Domain and Application.
- **API**: Controllers, middleware, and application startup. Depends on Application and Infrastructure.

### Design Decisions

1. **CQRS with MediatR**: Separates read and write operations for better scalability and maintainability.
2. **Repository Pattern**: Abstracts data access, making it easier to test and swap implementations.
3. **Unit of Work**: Ensures transactional consistency across multiple repository operations.
4. **Domain-Driven Design**: Business logic encapsulated in domain entities with rich behavior.
5. **SQLite Database**: Chosen for simplicity and portability. Can easily be swapped for PostgreSQL or SQL Server.

### Business Rules Implementation

- **Cutoff Time Validation**: Orders are rejected if created after the canteen's cutoff time for the fulfilment date.
- **Stock Validation**: Orders are rejected if requested quantities exceed available daily stock.
- **Wallet Balance Validation**: Orders are rejected if parent's wallet balance is insufficient.
- **Allergen Validation**: Orders are rejected if any menu item contains an allergen that the student is allergic to.
- **Idempotency**: Supports 24-hour idempotency window via `Idempotency-Key` header to prevent duplicate orders.

### Trade-offs

1. **Database Provider**: SQLite chosen for simplicity. In production, would use PostgreSQL or SQL Server with proper connection pooling.
2. **In-Memory Testing**: Integration tests use EF Core InMemory provider. For more realistic tests, would use Testcontainers with actual database.
3. **Transaction Scope**: Currently uses database transactions. For distributed scenarios, would implement saga pattern or outbox pattern.
4. **Concurrency**: Basic optimistic concurrency. For high-contention scenarios, would implement row-level locking or event sourcing.
5. **Authentication**: Currently stubbed. Designed to support future JWT-based authentication with parent-student authorization.

### What Would Be Done Next

1. **Optimistic Concurrency**: Add concurrency tokens to prevent race conditions on stock and wallet updates.
2. **Outbox Pattern**: Implement transactional outbox for reliable event publishing to downstream systems (POS integration).
3. **Health Checks**: Add health check endpoints for database connectivity and external dependencies.
4. **Metrics & Observability**: Integrate Application Insights or Prometheus for metrics, distributed tracing, and correlation IDs.
5. **API Versioning**: Implement API versioning strategy for backward compatibility.
6. **Rate Limiting**: Add rate limiting to prevent abuse.
7. **Caching**: Implement caching for frequently accessed data (canteen schedules, menu items).
8. **Background Jobs**: Add background job processing for order fulfilment notifications and stock replenishment.
9. **Timezone Handling**: Proper timezone handling for Sydney/Australia timezone for cutoff times.
10. **Refund Processing**: Implement refund workflow for cancelled orders.
11. **Application Service**: Implement a dedicated Application Service for clean decoupling and lean handlers.


## How to Run Locally

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022, VS Code, or Rider

### Setup

1. Clone the repository or extract the ZIP file.

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

4. Run the API:
   ```bash
   cd src/API/API
   dotnet run
   ```

5. The API will be available at:
   - HTTP: `http://localhost:5244`
   - HTTPS: `https://localhost:7244`
   - Swagger UI: `https://localhost:7244/swagger`

### Running Tests

Run all tests:
```bash
dotnet test
```

Run specific test project:
```bash
dotnet test tests/Domain.Tests/Domain.Tests/Domain.Tests.csproj
dotnet test tests/Application.Tests/Application.Tests/Application.Tests.csproj
dotnet test tests/Integration.Tests/Integration.Tests/Integration.Tests.csproj
```

## API Endpoints

### POST /api/orders

Creates a new order.

**Headers:**
- `Idempotency-Key` (optional): Ensures idempotent requests within 24 hours.

**Request Body:**
```json
{
  "parentId": "11111111-1111-1111-1111-111111111111",
  "studentId": "22222222-2222-2222-2222-222222222222",
  "canteenId": "33333333-3333-3333-3333-333333333333",
  "fulfilmentDate": "2025-11-16T00:00:00Z",
  "items": [
    {
      "menuItemId": "44444444-4444-4444-4444-444444444444",
      "quantity": 2
    }
  ]
}
```

**Response:** 201 Created with order details

**Error Responses:**
- 400 Bad Request: Validation failure (cutoff time, stock, wallet balance, allergens)
- 500 Internal Server Error: Unexpected error

### GET /api/orders/{id}

Retrieves order details by ID.

**Response:** 200 OK with order details, or 404 Not Found

## Project Structure

```
src/
  Domain/
    Entities/          # Domain entities (Parent, Student, Canteen, MenuItem, Order)
    Interfaces/        # Repository and service interfaces
    Exceptions/        # Domain exceptions
    ValueObjects/     # Value objects
  Application/
    Commands/         # CQRS commands
    Queries/          # CQRS queries
    Handlers/         # Command and query handlers
    DTOs/             # Data transfer objects
    Interfaces/       # Application service interfaces
    Services/         # Application services
  Infrastructure/
    Data/             # EF Core DbContext
    Repositories/     # Repository implementations
  API/
    Controllers/      # API controllers
    Program.cs        # Application startup

tests/
  Domain.Tests/       # Unit tests for domain logic
  Application.Tests/ # Unit tests for application logic
  Integration.Tests/ # Integration tests for API endpoints
```

## Configuration

Database connection string can be configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=..\\..\\Database\\ordering.db"
  }
}
```

For production, use environment variables or Azure Key Vault for sensitive configuration.

