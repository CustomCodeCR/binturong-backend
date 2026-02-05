using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Products.Create;

internal sealed class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public CreateProductCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken ct)
    {
        var sku = command.SKU.Trim();
        var barcode = string.IsNullOrWhiteSpace(command.Barcode) ? null : command.Barcode.Trim();

        if (command.BasePrice < 0)
            return Result.Failure<Guid>(ProductErrors.InvalidPrice);
        if (command.AverageCost < 0)
            return Result.Failure<Guid>(ProductErrors.InvalidAverageCost);

        var skuExists = await _db.Products.AnyAsync(x => x.SKU == sku, ct);
        if (skuExists)
            return Result.Failure<Guid>(ProductErrors.SkuNotUnique);

        if (barcode is not null)
        {
            var barcodeExists = await _db.Products.AnyAsync(x => x.Barcode == barcode, ct);
            if (barcodeExists)
                return Result.Failure<Guid>(ProductErrors.BarcodeNotUnique);
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

        return Result.Success(product.Id);
    }
}
