namespace Api.Endpoints.Branches;

public sealed record CreateBranchRequest(
    string Code,
    string Name,
    string Address,
    string Phone,
    bool IsActive = true
);

public sealed record UpdateBranchRequest(
    string Code,
    string Name,
    string Address,
    string Phone,
    bool IsActive
);
