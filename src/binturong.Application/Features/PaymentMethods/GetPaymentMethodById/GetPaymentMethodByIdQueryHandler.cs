using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.MasterData;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.PaymentMethods.GetPaymentMethodById;

internal sealed class GetPaymentMethodByIdQueryHandler
    : IQueryHandler<GetPaymentMethodByIdQuery, PaymentMethodReadModel>
{
    private const string CollectionName = "payment_methods";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetPaymentMethodByIdQueryHandler(
        IMongoDatabase db,
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

    public async Task<Result<PaymentMethodReadModel>> Handle(
        GetPaymentMethodByIdQuery q,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<PaymentMethodReadModel>(CollectionName);

        var id = $"pm:{q.PaymentMethodId}";
        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "PaymentMethods",
            "PaymentMethod",
            q.PaymentMethodId,
            "PAYMENT_METHOD_READ",
            string.Empty,
            $"paymentMethodId={q.PaymentMethodId}; found={(doc is not null)}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return doc is null
            ? Result.Failure<PaymentMethodReadModel>(
                Error.NotFound(
                    "PaymentMethods.NotFound",
                    $"PaymentMethod '{q.PaymentMethodId}' not found."
                )
            )
            : Result.Success(doc);
    }
}
