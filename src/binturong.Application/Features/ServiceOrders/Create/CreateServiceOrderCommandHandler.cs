using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.ServiceOrderChecklists;
using Domain.ServiceOrderMaterials;
using Domain.ServiceOrderPhotos;
using Domain.ServiceOrders;
using Domain.ServiceOrderServices;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.ServiceOrders.Create;

internal sealed class CreateServiceOrderCommandHandler
    : ICommandHandler<CreateServiceOrderCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateServiceOrderCommandHandler(
        IApplicationDbContext db,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateServiceOrderCommand cmd, CancellationToken ct)
    {
        if (cmd.ClientId == Guid.Empty)
            return Result.Failure<Guid>(ServiceOrderErrors.ClientRequired);

        if (cmd.ScheduledDate == default)
            return Result.Failure<Guid>(ServiceOrderErrors.ScheduledDateRequired);

        if (string.IsNullOrWhiteSpace(cmd.ServiceAddress))
            return Result.Failure<Guid>(ServiceOrderErrors.ServiceAddressRequired);

        if (cmd.Services is null || cmd.Services.Count == 0)
            return Result.Failure<Guid>(ServiceOrderErrors.ServiceRequired);

        var order = new ServiceOrder
        {
            Id = Guid.NewGuid(),
            Code = cmd.Code.Trim(),
            ClientId = cmd.ClientId,
            BranchId = cmd.BranchId,
            ContractId = cmd.ContractId,
            ScheduledDate = EnsureUtc(cmd.ScheduledDate),
            Status = "Pending",
            ServiceAddress = cmd.ServiceAddress.Trim(),
            Notes = cmd.Notes?.Trim() ?? string.Empty,
        };

        order.RaiseCreated();

        foreach (var line in cmd.Services)
        {
            var service = await _db.Services.FirstOrDefaultAsync(x => x.Id == line.ServiceId, ct);
            if (service is null)
                return Result.Failure<Guid>(Domain.Services.ServiceErrors.NotFound(line.ServiceId));

            var available = service.ValidateAvailability();
            if (available.IsFailure)
                return Result.Failure<Guid>(available.Error);

            var quantity = line.Quantity <= 0 ? 1 : line.Quantity;

            var orderService = new ServiceOrderService
            {
                Id = Guid.NewGuid(),
                ServiceOrderId = order.Id,
                ServiceId = service.Id,
                Quantity = quantity,
                RateApplied = service.BaseRate,
                LineTotal = service.BaseRate * quantity,
            };

            order.AddService(orderService, service.Name);
        }

        foreach (var item in cmd.Materials ?? [])
        {
            var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == item.ProductId, ct);
            if (product is null)
                return Result.Failure<Guid>(
                    Error.NotFound("Products.NotFound", $"Product '{item.ProductId}' not found.")
                );

            var material = new ServiceOrderMaterial
            {
                Id = Guid.NewGuid(),
                ServiceOrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity <= 0 ? 1 : item.Quantity,
                EstimatedCost = item.EstimatedCost < 0 ? 0 : item.EstimatedCost,
            };

            order.AddMaterial(material, product.Name);
        }

        foreach (var item in cmd.Checklists ?? [])
        {
            if (string.IsNullOrWhiteSpace(item.Description))
                continue;

            order.AddChecklist(
                new ServiceOrderChecklist
                {
                    Id = Guid.NewGuid(),
                    ServiceOrderId = order.Id,
                    Description = item.Description.Trim(),
                    IsCompleted = item.IsCompleted,
                }
            );
        }

        foreach (var item in cmd.Photos ?? [])
        {
            if (string.IsNullOrWhiteSpace(item.PhotoS3Key))
                continue;

            order.AddPhoto(
                new ServiceOrderPhoto
                {
                    Id = Guid.NewGuid(),
                    ServiceOrderId = order.Id,
                    PhotoS3Key = item.PhotoS3Key.Trim(),
                    Description = item.Description?.Trim() ?? string.Empty,
                }
            );
        }

        _db.ServiceOrders.Add(order);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "ServiceOrders",
            "ServiceOrder",
            order.Id,
            "SERVICE_ORDER_CREATED",
            string.Empty,
            $"code={order.Code}; clientId={order.ClientId}; contractId={order.ContractId}; services={order.Services.Count}; materials={order.Materials.Count}; checklists={order.Checklists.Count}; photos={order.Photos.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(order.Id);
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };
    }
}
