using SharedKernel;

namespace Application.Abstractions.Security;

public interface IAdminPasswordResetService
{
    Task<Result> ResetAdminPasswordAsync(string newPassword, CancellationToken ct);
}
