namespace Dita.Shared.Identity.Enums;

/// <summary>
/// Represents the possible outcomes of a login attempt.
/// </summary>
public enum LoginResponse
{
   /// <summary>
   /// The login attempt was successful.
   /// </summary>
   Success,

   /// <summary>
   /// The provided credentials (username or password) were invalid.
   /// </summary>
   InvalidCredentials,

   /// <summary>
   /// No user account matching the provided identifier was found.
   /// </summary>
   UserNotFound,

   /// <summary>
   /// The user account has been temporarily locked out due to too many failed attempts.
   /// </summary>
   LockedOut,

   /// <summary>
   /// The user account has been permanently banned and is not permitted to log in.
   /// </summary>
   Banned,

   /// <summary>
   /// Two-factor authentication is required to complete the login.
   /// </summary>
   TwoFactorRequired,

   /// <summary>
   /// An unexpected error occurred during the login process.
   /// </summary>
   UnknownError
}
