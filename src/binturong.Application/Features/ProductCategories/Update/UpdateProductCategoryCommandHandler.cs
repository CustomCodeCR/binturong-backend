using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.ProductCategories;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.ProductCategories.Update;

internal sealed class UpdateProductCategoryCommandHandler
    : ICommandHandler<UpdateProductCategoryCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateProductCategoryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateProductCategoryCommand command, CancellationToken ct)
    {
        var category = await _db.ProductCategories.FirstOrDefaultAsync(
            x => x.Id == command.CategoryId,
            ct
        );
        if (category is null)
            return Result.Failure(ProductCategoryErrors.NotFound(command.CategoryId));

        var name = command.Name.Trim();

        var exists = await _db.ProductCategories.AnyAsync(
            x => x.Id != command.CategoryId && x.Name == name,
            ct
        );
        if (exists)
            return Result.Failure(ProductCategoryErrors.NameNotUnique);

        category.Name = name;
        category.Description = command.Description?.Trim() ?? "";
        category.IsActive = command.IsActive;

        category.RaiseUpdated();

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
