namespace Dita.Shared.Identity.Enums;

public enum LoginResponse
{
   Success,
   InvalidCredentials,
   UserNotFound,
   LockedOut,
   Banned,
   TwoFactorRequired,
   UnknownError
}
