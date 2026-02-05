using SharedKernel;

namespace Application.Features.Auth.Login;

public static class LoginErrors
{
    public static readonly Error InvalidCredentials = Error.Failure(
        "Auth.InvalidCredentials",
        "Invalid username/email or password"
    );

    public static readonly Error UserInactive = Error.Failure(
        "Auth.UserInactive",
        "User is inactive"
    );

    public static readonly Error UserLocked = Error.Failure(
        "Auth.UserLocked",
        "User is temporarily locked"
    );
}
