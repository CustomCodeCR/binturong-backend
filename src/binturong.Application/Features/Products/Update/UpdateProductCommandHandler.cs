using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Products.Update;

internal sealed class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateProductCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateProductCommand command, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == command.ProductId, ct);
        if (product is null)
            return Result.Failure(ProductErrors.NotFound(command.ProductId));

        var sku = command.SKU.Trim();
        var barcode = string.IsNullOrWhiteSpace(command.Barcode) ? null : command.Barcode.Trim();

        if (command.BasePrice < 0)
            return Result.Failure(ProductErrors.InvalidPrice);
        if (command.AverageCost < 0)
            return Result.Failure(ProductErrors.InvalidAverageCost);

        var skuExists = await _db.Products.AnyAsync(
            x => x.Id != command.ProductId && x.SKU == sku,
            ct
        );
        if (skuExists)
            return Result.Failure(ProductErrors.SkuNotUnique);

        if (barcode is not null)
        {
            var barcodeExists = await _db.Products.AnyAsync(
                x => x.Id != command.ProductId && x.Barcode == barcode,
                ct
            );
            if (barcodeExists)
                return Result.Failure(ProductErrors.BarcodeNotUnique);
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
        return Result.Success();
    }
}
