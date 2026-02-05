using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.ProductCategories;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.ProductCategories.Delete;

internal sealed class DeleteProductCategoryCommandHandler
    : ICommandHandler<DeleteProductCategoryCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteProductCategoryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteProductCategoryCommand command, CancellationToken ct)
    {
        var category = await _db.ProductCategories.FirstOrDefaultAsync(
            x => x.Id == command.CategoryId,
            ct
        );
        if (category is null)
            return Result.Failure(ProductCategoryErrors.NotFound(command.CategoryId));

        category.RaiseDeleted();

        _db.ProductCategories.Remove(category);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
