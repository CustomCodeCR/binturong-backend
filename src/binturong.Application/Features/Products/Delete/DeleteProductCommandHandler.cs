using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Products.Delete;

internal sealed class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public DeleteProductCommandHandler(
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

    public async Task<Result> Handle(DeleteProductCommand command, CancellationToken ct)
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
                "PRODUCT_DELETE_FAILED",
                string.Empty,
                $"reason=not_found; productId={command.ProductId}",
                ip,
                ua,
                ct
            );
            return Result.Failure(ProductErrors.NotFound(command.ProductId));
        }

        var before =
            $"sku={product.SKU}; barcode={product.Barcode}; name={product.Name}; isActive={product.IsActive}; isService={product.IsService}";

        product.RaiseDeleted();

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Products",
            "Product",
            product.Id,
            "PRODUCT_DELETED",
            before,
            $"productId={product.Id}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
