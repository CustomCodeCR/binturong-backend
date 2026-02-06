using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.ProductCategories;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.ProductCategories.Update;

internal sealed class UpdateProductCategoryCommandHandler
    : ICommandHandler<UpdateProductCategoryCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UpdateProductCategoryCommandHandler(
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

    public async Task<Result> Handle(UpdateProductCategoryCommand command, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var category = await _db.ProductCategories.FirstOrDefaultAsync(
            x => x.Id == command.CategoryId,
            ct
        );

        if (category is null)
        {
            await _bus.AuditAsync(
                userId,
                "ProductCategories",
                "ProductCategory",
                command.CategoryId,
                "CATEGORY_UPDATE_FAILED",
                string.Empty,
                $"reason=not_found; categoryId={command.CategoryId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(ProductCategoryErrors.NotFound(command.CategoryId));
        }

        var before =
            $"name={category.Name}; description={category.Description}; isActive={category.IsActive}";

        var name = command.Name.Trim();

        var exists = await _db.ProductCategories.AnyAsync(
            x => x.Id != command.CategoryId && x.Name == name,
            ct
        );

        if (exists)
        {
            await _bus.AuditAsync(
                userId,
                "ProductCategories",
                "ProductCategory",
                category.Id,
                "CATEGORY_UPDATE_FAILED",
                before,
                $"reason=name_not_unique; newName={name}",
                ip,
                ua,
                ct
            );

            return Result.Failure(ProductCategoryErrors.NameNotUnique);
        }

        category.Name = name;
        category.Description = command.Description?.Trim() ?? "";
        category.IsActive = command.IsActive;

        category.RaiseUpdated();

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "ProductCategories",
            "ProductCategory",
            category.Id,
            "CATEGORY_UPDATED",
            before,
            $"name={category.Name}; description={category.Description}; isActive={category.IsActive}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
