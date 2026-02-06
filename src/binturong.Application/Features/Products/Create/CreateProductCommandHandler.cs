using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Products.Create;

internal sealed class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateProductCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;

        var sku = command.SKU.Trim();
        var barcode = string.IsNullOrWhiteSpace(command.Barcode) ? null : command.Barcode.Trim();

        if (command.BasePrice < 0)
        {
            await _bus.AuditAsync(
                userId,
                "Products",
                "Product",
                null,
                "PRODUCT_CREATE_FAILED",
                string.Empty,
                "reason=invalid_price",
                ip,
                ua,
                ct
            );
            return Result.Failure<Guid>(ProductErrors.InvalidPrice);
        }

        if (command.AverageCost < 0)
        {
            await _bus.AuditAsync(
                userId,
                "Products",
                "Product",
                null,
                "PRODUCT_CREATE_FAILED",
                string.Empty,
                "reason=invalid_average_cost",
                ip,
                ua,
                ct
            );
            return Result.Failure<Guid>(ProductErrors.InvalidAverageCost);
        }

        var skuExists = await _db.Products.AnyAsync(x => x.SKU == sku, ct);
        if (skuExists)
        {
            await _bus.AuditAsync(
                userId,
                "Products",
                "Product",
                null,
                "PRODUCT_CREATE_FAILED",
                string.Empty,
                $"reason=sku_not_unique; sku={sku}",
                ip,
                ua,
                ct
            );
            return Result.Failure<Guid>(ProductErrors.SkuNotUnique);
        }

        if (barcode is not null)
        {
            var barcodeExists = await _db.Products.AnyAsync(x => x.Barcode == barcode, ct);
            if (barcodeExists)
            {
                await _bus.AuditAsync(
                    userId,
                    "Products",
                    "Product",
                    null,
                    "PRODUCT_CREATE_FAILED",
                    string.Empty,
                    $"reason=barcode_not_unique; barcode={barcode}",
                    ip,
                    ua,
                    ct
                );
                return Result.Failure<Guid>(ProductErrors.BarcodeNotUnique);
            }
        }

        var now = DateTime.UtcNow;

        var product = new Product
        {
            Id = Guid.NewGuid(),
            SKU = sku,
            Barcode = barcode ?? "",
            Name = command.Name.Trim(),
            Description = command.Description?.Trim() ?? "",
            CategoryId = command.CategoryId,
            UomId = command.UomId,
            TaxId = command.TaxId,
            BasePrice = command.BasePrice,
            AverageCost = command.AverageCost,
            IsService = command.IsService,
            IsActive = command.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
        };

        product.RaiseCreated();

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Products",
            "Product",
            product.Id,
            "PRODUCT_CREATED",
            string.Empty,
            $"productId={product.Id}; sku={product.SKU}; name={product.Name}; isActive={product.IsActive}; isService={product.IsService}",
            ip,
            ua,
            ct
        );

        return Result.Success(product.Id);
    }
}
