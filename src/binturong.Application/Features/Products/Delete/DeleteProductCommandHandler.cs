using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Products.Delete;

internal sealed class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteProductCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteProductCommand command, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == command.ProductId, ct);
        if (product is null)
            return Result.Failure(ProductErrors.NotFound(command.ProductId));

        product.RaiseDeleted();

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
