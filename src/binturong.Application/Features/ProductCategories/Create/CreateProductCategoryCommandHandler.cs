using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.ProductCategories;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.ProductCategories.Create;

internal sealed class CreateProductCategoryCommandHandler
    : ICommandHandler<CreateProductCategoryCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateProductCategoryCommandHandler(
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

    public async Task<Result<Guid>> Handle(
        CreateProductCategoryCommand command,
        CancellationToken ct
    )
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var name = command.Name.Trim();

        var exists = await _db.ProductCategories.AnyAsync(x => x.Name == name, ct);
        if (exists)
        {
            await _bus.AuditAsync(
                userId,
                "ProductCategories",
                "ProductCategory",
                null,
                "CATEGORY_CREATE_FAILED",
                string.Empty,
                $"reason=name_not_unique; name={name}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(ProductCategoryErrors.NameNotUnique);
        }

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

        await _bus.AuditAsync(
            userId,
            "ProductCategories",
            "ProductCategory",
            category.Id,
            "CATEGORY_CREATED",
            string.Empty,
            $"categoryId={category.Id}; name={category.Name}; isActive={category.IsActive}",
            ip,
            ua,
            ct
        );

        return Result.Success(category.Id);
    }
}
