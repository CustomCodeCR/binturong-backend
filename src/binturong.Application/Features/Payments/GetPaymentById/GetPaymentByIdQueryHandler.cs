using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Payments.GetPaymentById;

internal sealed class GetPaymentByIdQueryHandler
    : IQueryHandler<GetPaymentByIdQuery, PaymentReadModel>
{
    private readonly IMongoDatabase _mongo;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetPaymentByIdQueryHandler(
        IMongoDatabase mongo,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _mongo = mongo;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result<PaymentReadModel>> Handle(GetPaymentByIdQuery q, CancellationToken ct)
    {
        var col = _mongo.GetCollection<PaymentReadModel>(MongoCollections.Payments);
        var doc = await col.Find(x => x.Id == $"payment:{q.Id}").FirstOrDefaultAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Payments",
            "Payment",
            q.Id,
            "PAYMENT_READ",
            string.Empty,
            doc is null ? "not_found" : "ok",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return doc is null
            ? Result.Failure<PaymentReadModel>(
                Error.NotFound("Payments.NotFound", $"Payment '{q.Id}' not found.")
            )
            : Result.Success(doc);
    }
}
