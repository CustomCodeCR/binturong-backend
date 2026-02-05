using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.ProductCategories;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.ProductCategories.Create;

internal sealed class CreateProductCategoryCommandHandler
    : ICommandHandler<CreateProductCategoryCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public CreateProductCategoryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(
        CreateProductCategoryCommand command,
        CancellationToken ct
    )
    {
        var name = command.Name.Trim();

        var exists = await _db.ProductCategories.AnyAsync(x => x.Name == name, ct);
        if (exists)
            return Result.Failure<Guid>(ProductCategoryErrors.NameNotUnique);

        var category = new ProductCategory
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = command.Description?.Trim() ?? "",
            IsActive = command.IsActive,
        };

        category.RaiseCreated();

        _db.ProductCategories.Add(category);
        await _db.SaveChangesAsync(ct);

        return Result.Success(category.Id);
    }
}
