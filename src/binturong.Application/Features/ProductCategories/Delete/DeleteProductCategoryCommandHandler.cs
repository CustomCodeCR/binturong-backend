using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.ProductCategories;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.ProductCategories.Delete;

internal sealed class DeleteProductCategoryCommandHandler
    : ICommandHandler<DeleteProductCategoryCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public DeleteProductCategoryCommandHandler(
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

    public async Task<Result> Handle(DeleteProductCategoryCommand command, CancellationToken ct)
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
                "CATEGORY_DELETE_FAILED",
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

        category.RaiseDeleted();

        _db.ProductCategories.Remove(category);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "ProductCategories",
            "ProductCategory",
            category.Id,
            "CATEGORY_DELETED",
            before,
            $"categoryId={category.Id}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
