using Application.Commands;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IParentRepository _parentRepository;
    private readonly IRepository<Student> _studentRepository;
    private readonly IRepository<Canteen> _canteenRepository;
    private readonly IRepository<MenuItem> _menuItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private static readonly TimeSpan IdempotencyWindow = TimeSpan.FromHours(24);

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IParentRepository parentRepository,
        IRepository<Student> studentRepository,
        IRepository<Canteen> canteenRepository,
        IRepository<MenuItem> menuItemRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _parentRepository = parentRepository;
        _studentRepository = studentRepository;
        _canteenRepository = canteenRepository;
        _menuItemRepository = menuItemRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating order for ParentId: {ParentId}, StudentId: {StudentId}, CanteenId: {CanteenId}",
            request.Order.ParentId, request.Order.StudentId, request.Order.CanteenId);

        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existingOrder = await _orderRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
            if (existingOrder != null)
            {
                _logger.LogInformation("Idempotent request detected. Returning existing order {OrderId}", existingOrder.Id);
                return MapToDto(existingOrder);
            }
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var parent = await _parentRepository.GetByIdAsync(request.Order.ParentId, cancellationToken);
            if (parent == null)
                throw new OrderValidationException($"Parent with id {request.Order.ParentId} not found");

            var student = await _studentRepository.GetByIdAsync(request.Order.StudentId, cancellationToken);
            if (student == null)
                throw new OrderValidationException($"Student with id {request.Order.StudentId} not found");

            if (student.ParentId != request.Order.ParentId)
                throw new OrderValidationException("Student does not belong to the specified parent");

            var canteen = await _canteenRepository.GetByIdAsync(request.Order.CanteenId, cancellationToken);
            if (canteen == null)
                throw new OrderValidationException($"Canteen with id {request.Order.CanteenId} not found");

            var fulfilmentDate = request.Order.FulfilmentDate.Date;
            var dayOfWeek = fulfilmentDate.DayOfWeek;

            if (!canteen.IsOpenOnDay(dayOfWeek))
                throw new OrderValidationException($"Canteen is not open on {dayOfWeek}");

            var cutoffTime = canteen.GetCutoffTimeForDay(dayOfWeek);
            if (cutoffTime.HasValue)
            {
                var currentTime = _dateTimeProvider.Now.TimeOfDay;
                if (currentTime > cutoffTime.Value)
                    throw new OrderValidationException($"Order cutoff time ({cutoffTime.Value}) has passed");
            }

            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var itemDto in request.Order.Items)
            {
                var menuItem = await _menuItemRepository.GetByIdAsync(itemDto.MenuItemId, cancellationToken);
                if (menuItem == null)
                    throw new OrderValidationException($"MenuItem with id {itemDto.MenuItemId} not found");

                if (menuItem.CanteenId != request.Order.CanteenId)
                    throw new OrderValidationException($"MenuItem {menuItem.Id} does not belong to the specified canteen");

                if (!menuItem.IsInStock(itemDto.Quantity))
                    throw new OrderValidationException($"Insufficient stock for menu item {menuItem.Name}");

                if (!string.IsNullOrEmpty(student.Allergen) && menuItem.HasAllergen(student.Allergen))
                    throw new OrderValidationException($"Menu item {menuItem.Name} contains allergen {student.Allergen} which the student is allergic to");

                var itemTotal = menuItem.Price * itemDto.Quantity;
                _logger.LogInformation("Adding menu item {MenuItemId} qty {Quantity} price {Price} total {ItemTotal}",
                    menuItem.Id, itemDto.Quantity, menuItem.Price, itemTotal);
                totalAmount += itemTotal;

                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    MenuItemId = menuItem.Id,
                    MenuItem = menuItem,
                    Quantity = itemDto.Quantity,
                    UnitPrice = menuItem.Price
                });
            }

            if (parent.WalletBalance < totalAmount)
                throw new OrderValidationException($"Insufficient wallet balance. Required: {totalAmount}, Available: {parent.WalletBalance}");

            var order = new Order
            {
                Id = Guid.NewGuid(),
                ParentId = parent.Id,
                Parent = parent,
                StudentId = student.Id,
                Student = student,
                CanteenId = canteen.Id,
                Canteen = canteen,
                FulfilmentDate = fulfilmentDate,
                CreatedAt = _dateTimeProvider.UtcNow,
                Status = OrderStatus.Placed,
                TotalAmount = totalAmount,
                IdempotencyKey = request.IdempotencyKey,
                Items = orderItems
            };

            foreach (var item in orderItems)
            {
                item.OrderId = order.Id;
                item.Order = order;
                item.MenuItem.DecrementStock(item.Quantity);
            }

            parent.DebitWallet(totalAmount);
            order.Confirm();

            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} created successfully. Total: {TotalAmount}", order.Id, totalAmount);

            return MapToDto(order);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            ParentId = order.ParentId,
            StudentId = order.StudentId,
            CanteenId = order.CanteenId,
            FulfilmentDate = order.FulfilmentDate,
            CreatedAt = order.CreatedAt,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(item => new OrderItemDetailDto
            {
                MenuItemId = item.MenuItemId,
                MenuItemName = item.MenuItem.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };
    }
}

