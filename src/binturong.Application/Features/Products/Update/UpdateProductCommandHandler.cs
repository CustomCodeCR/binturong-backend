using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Products.Update;

internal sealed class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UpdateProductCommandHandler(
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

    public async Task<Result> Handle(UpdateProductCommand command, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;

        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == command.ProductId, ct);
        if (product is null)
        {
            await _bus.AuditAsync(
                userId,
                "Products",
                "Product",
                command.ProductId,
                "PRODUCT_UPDATE_FAILED",
                string.Empty,
                $"reason=not_found; productId={command.ProductId}",
                ip,
                ua,
                ct
            );
            return Result.Failure(ProductErrors.NotFound(command.ProductId));
        }

        var before =
            $"sku={product.SKU}; barcode={product.Barcode}; name={product.Name}; basePrice={product.BasePrice}; averageCost={product.AverageCost}; isActive={product.IsActive}; isService={product.IsService}";

        var sku = command.SKU.Trim();
        var barcode = string.IsNullOrWhiteSpace(command.Barcode) ? null : command.Barcode.Trim();

        if (command.BasePrice < 0)
        {
            await _bus.AuditAsync(
                userId,
                "Products",
                "Product",
                product.Id,
                "PRODUCT_UPDATE_FAILED",
                before,
                "reason=invalid_price",
                ip,
                ua,
                ct
            );
            return Result.Failure(ProductErrors.InvalidPrice);
        }

        if (command.AverageCost < 0)
        {
            await _bus.AuditAsync(
                userId,
                "Products",
                "Product",
                product.Id,
                "PRODUCT_UPDATE_FAILED",
                before,
                "reason=invalid_average_cost",
                ip,
                ua,
                ct
            );
            return Result.Failure(ProductErrors.InvalidAverageCost);
        }

        var skuExists = await _db.Products.AnyAsync(
            x => x.Id != command.ProductId && x.SKU == sku,
            ct
        );
        if (skuExists)
        {
            await _bus.AuditAsync(
                userId,
                "Products",
                "Product",
                product.Id,
                "PRODUCT_UPDATE_FAILED",
                before,
                $"reason=sku_not_unique; newSku={sku}",
                ip,
                ua,
                ct
            );
            return Result.Failure(ProductErrors.SkuNotUnique);
        }

        if (barcode is not null)
        {
            var barcodeExists = await _db.Products.AnyAsync(
                x => x.Id != command.ProductId && x.Barcode == barcode,
                ct
            );
            if (barcodeExists)
            {
                await _bus.AuditAsync(
                    userId,
                    "Products",
                    "Product",
                    product.Id,
                    "PRODUCT_UPDATE_FAILED",
                    before,
                    $"reason=barcode_not_unique; newBarcode={barcode}",
                    ip,
                    ua,
                    ct
                );
                return Result.Failure(ProductErrors.BarcodeNotUnique);
            }
        }

        product.SKU = sku;
        product.Barcode = barcode ?? "";
        product.Name = command.Name.Trim();
        product.Description = command.Description?.Trim() ?? "";
        product.CategoryId = command.CategoryId;
        product.UomId = command.UomId;
        product.TaxId = command.TaxId;
        product.BasePrice = command.BasePrice;
        product.AverageCost = command.AverageCost;
        product.IsService = command.IsService;
        product.IsActive = command.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        product.RaiseUpdated();

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Products",
            "Product",
            product.Id,
            "PRODUCT_UPDATED",
            before,
            $"sku={product.SKU}; barcode={product.Barcode}; name={product.Name}; basePrice={product.BasePrice}; averageCost={product.AverageCost}; isActive={product.IsActive}; isService={product.IsService}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
