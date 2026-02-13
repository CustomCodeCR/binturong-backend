using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Invoices.Delete;

internal sealed class DeleteInvoiceCommandHandler : ICommandHandler<DeleteInvoiceCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public DeleteInvoiceCommandHandler(
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

    public async Task<Result> Handle(DeleteInvoiceCommand cmd, CancellationToken ct)
    {
        var invoice = await _db.Invoices.FirstOrDefaultAsync(x => x.Id == cmd.InvoiceId, ct);
        if (invoice is null)
            return Result.Failure(InvoiceErrors.NotFound(cmd.InvoiceId));

        invoice.RaiseDeleted();

        _db.Invoices.Remove(invoice);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Invoices",
            "Invoice",
            cmd.InvoiceId,
            "INVOICE_DELETED",
            string.Empty,
            "ok",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success();
    }
}
