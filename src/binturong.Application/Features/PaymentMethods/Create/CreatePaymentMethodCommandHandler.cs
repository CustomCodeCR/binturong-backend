using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.PaymentMethods;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.PaymentMethods.Create;

internal sealed class CreatePaymentMethodCommandHandler
    : ICommandHandler<CreatePaymentMethodCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreatePaymentMethodCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreatePaymentMethodCommand cmd, CancellationToken ct)
    {
        var code = cmd.Code.Trim();
        var desc = cmd.Description.Trim();

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Guid>(
                Error.Validation("PaymentMethods.CodeRequired", "Code is required.")
            );

        if (string.IsNullOrWhiteSpace(desc))
            return Result.Failure<Guid>(
                Error.Validation("PaymentMethods.DescriptionRequired", "Description is required.")
            );

        var exists = await _db.PaymentMethods.AnyAsync(x => x.Code == code, ct);
        if (exists)
            return Result.Failure<Guid>(
                Error.Conflict("PaymentMethods.CodeExists", $"Payment method code '{code}' exists.")
            );

        var pm = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            Code = code,
            Description = desc,
            IsActive = cmd.IsActive,
        };

        _db.PaymentMethods.Add(pm);
        pm.RaiseCreated();

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "PaymentMethods",
            "PaymentMethod",
            pm.Id,
            "PAYMENT_METHOD_CREATED",
            string.Empty,
            $"code={pm.Code}; active={pm.IsActive}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(pm.Id);
    }
}
