using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.PurchaseRequests;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Purchases.PurchaseRequests.Create;

internal sealed class CreatePurchaseRequestCommandHandler
    : ICommandHandler<CreatePurchaseRequestCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreatePurchaseRequestCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreatePurchaseRequestCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var code = cmd.Code.Trim();
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Guid>(PurchaseRequestErrors.CodeRequired);

        var codeExists = await _db.PurchaseRequests.AnyAsync(
            x => x.Code.ToLower() == code.ToLower(),
            ct
        );
        if (codeExists)
            return Result.Failure<Guid>(PurchaseRequestErrors.CodeNotUnique);

        var request = new PurchaseRequest
        {
            Id = Guid.NewGuid(),
            Code = code,
            BranchId = cmd.BranchId,
            RequestedById = cmd.RequestedById,
            RequestDate = cmd.RequestDateUtc,
            Status = "Pending",
            Notes = cmd.Notes?.Trim() ?? string.Empty,
        };

        // ✅ Raise domain event from entity (so projector/outbox can consume it)
        // Option A: direct event
        request.RaiseCreated();

        // Option B: if you added MarkCreated() returning Result
        // var created = request.MarkCreated();
        // if (created.IsFailure) return Result.Failure<Guid>(created.Error);

        _db.PurchaseRequests.Add(request);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Purchases",
            "PurchaseRequest",
            request.Id,
            "PURCHASE_REQUEST_CREATED",
            string.Empty,
            $"requestId={request.Id}; code={request.Code}; branchId={request.BranchId}; requestedById={request.RequestedById}; requestDate={request.RequestDate:o}; status={request.Status}",
            ip,
            ua,
            ct
        );

        return Result.Success(request.Id);
    }
}
