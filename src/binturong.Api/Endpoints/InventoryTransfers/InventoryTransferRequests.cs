namespace Api.Endpoints.InventoryTransfers;

public sealed record CreateInventoryTransferRequest(
    Guid FromBranchId,
    Guid ToBranchId,
    string? Notes,
    Guid CreatedByUserId,
    IReadOnlyList<CreateInventoryTransferLineRequest> Lines
);

public sealed record CreateInventoryTransferLineRequest(
    Guid ProductId,
    decimal Quantity,
    Guid FromWarehouseId,
    Guid ToWarehouseId
);

public sealed record ApproveInventoryTransferRequest(Guid ApprovedByUserId);

public sealed record RejectInventoryTransferRequest(Guid RejectedByUserId, string? Reason);

public sealed record ConfirmInventoryTransferRequest(bool RequireApproval = false);
