using Application.Abstractions.Projections;
using Application.ReadModels.MasterData;
using Application.ReadModels.Payroll;
using Application.ReadModels.Purchases;
using Domain.PurchaseRequests;
using MongoDB.Driver;

namespace Infrastructure.Projections.Purchases;

internal sealed class PurchaseRequestProjection : IProjector<PurchaseRequestCreatedDomainEvent>
{
    private const string RequestsCol = "purchase_requests";
    private const string BranchesCol = "branches";
    private const string EmployeesCol = "employees";

    private readonly IMongoDatabase _db;

    public PurchaseRequestProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(PurchaseRequestCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PurchaseRequestReadModel>(RequestsCol);

        var branchName = e.BranchId.HasValue ? await ResolveBranchName(e.BranchId.Value, ct) : null;

        // Ajustá si tu EmployeeReadModel se llama distinto / está en otro namespace
        var requestedByName = e.RequestedById.HasValue
            ? await ResolveEmployeeName(e.RequestedById.Value, ct)
            : string.Empty;

        var doc = new PurchaseRequestReadModel
        {
            Id = $"purchase_request:{e.RequestId}",
            RequestId = e.RequestId,
            Code = e.Code,
            BranchId = e.BranchId,
            BranchName = branchName,
            RequestedById = e.RequestedById ?? Guid.Empty, // ReadModel lo tiene no-null: set empty if null
            RequestedByName = requestedByName,
            RequestDate = e.RequestDateUtc,
            Status = e.Status,
            Notes = e.Notes,
        };

        await col.InsertOneAsync(doc, new InsertOneOptions(), ct);
    }

    private async Task<string?> ResolveBranchName(Guid branchId, CancellationToken ct)
    {
        var col = _db.GetCollection<BranchReadModel>(BranchesCol);
        var b = await col.Find(x => x.BranchId == branchId).FirstOrDefaultAsync(ct);
        return b?.Name;
    }

    private async Task<string> ResolveEmployeeName(Guid employeeId, CancellationToken ct)
    {
        // Si tu read model de empleados NO existe así, cambia el tipo/colección
        var col = _db.GetCollection<EmployeeReadModel>(EmployeesCol);
        var emp = await col.Find(x => x.EmployeeId == employeeId).FirstOrDefaultAsync(ct);
        return emp?.FullName ?? string.Empty;
    }
}
