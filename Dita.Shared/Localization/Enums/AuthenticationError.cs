namespace Dita.Shared.Localization.Enums;

/// <summary>
/// Authentication and authorization-related error codes (range 6000-6999).
/// </summary>
public enum AuthenticationError
{
   /// <summary>
   /// Access is forbidden (insufficient permissions).
   /// </summary>
   AccessForbidden = 6001,

   /// <summary>
   /// Account has been disabled or deactivated.
   /// </summary>
   AccountDisabled = 6002,

   /// <summary>
   /// Account has expired.
   /// </summary>
   AccountExpired = 6003,

   /// <summary>
   /// Account is locked due to security policy (e.g., too many failed attempts).
   /// </summary>
   AccountLocked = 6004,

   /// <summary>
   /// Account does not exist.
   /// </summary>
   AccountNotFound = 6005,

   /// <summary>
   /// API key is invalid or has been revoked.
   /// </summary>
   ApiKeyInvalid = 6006,

   /// <summary>
   /// Authentication failed due to invalid credentials.
   /// </summary>
   AuthenticationFailed = 6007,

   /// <summary>
   /// Biometric authentication failed (fingerprint, face recognition, etc.).
   /// </summary>
   BiometricAuthenticationFailed = 6008,

   /// <summary>
   /// Certificate-based authentication failed.
   /// </summary>
   CertificateAuthenticationFailed = 6009,

   /// <summary>
   /// Email verification is required before authentication.
   /// </summary>
   EmailVerificationRequired = 6010,

   /// <summary>
   /// Incorrect or invalid password.
   /// </summary>
   InvalidPassword = 6011,

   /// <summary>
   /// Invalid authentication token.
   /// </summary>
   InvalidToken = 6012,

   /// <summary>
   /// Invalid username or user identifier.
   /// </summary>
   InvalidUsername = 6013,

   /// <summary>
   /// Multi-factor authentication (MFA) failed.
   /// </summary>
   MfaFailed = 6014,

   /// <summary>
   /// Multi-factor authentication (MFA) is required but not provided.
   /// </summary>
   MfaRequired = 6015,

   /// <summary>
   /// No error occurred (success).
   /// </summary>
   None = 6000,

   /// <summary>
   /// OAuth authentication or authorization failed.
   /// </summary>
   OAuthFailed = 6016,

   /// <summary>
   /// Password has expired and must be changed.
   /// </summary>
   PasswordExpired = 6017,

   /// <summary>
   /// Password reset is required before authentication.
   /// </summary>
   PasswordResetRequired = 6018,

   /// <summary>
   /// Insufficient permissions for the requested operation.
   /// </summary>
   PermissionDenied = 6019,

   /// <summary>
   /// Refresh token is invalid or expired.
   /// </summary>
   RefreshTokenInvalid = 6020,

   /// <summary>
   /// Session has expired.
   /// </summary>
   SessionExpired = 6021,

   /// <summary>
   /// Session does not exist or is invalid.
   /// </summary>
   SessionInvalid = 6022,

   /// <summary>
   /// Single sign-on (SSO) authentication failed.
   /// </summary>
   SsoFailed = 6023,

   /// <summary>
   /// Authentication token has expired.
   /// </summary>
   TokenExpired = 6024,

   /// <summary>
   /// Two-factor authentication code is invalid.
   /// </summary>
   TwoFactorCodeInvalid = 6025,

   /// <summary>
   /// User is not authenticated.
   /// </summary>
   Unauthenticated = 6026,

   /// <summary>
   /// User is not authorized to perform the operation.
   /// </summary>
   Unauthorized = 6027,

   /// <summary>
   /// Unknown authentication error occurred.
   /// </summary>
   UnknownAuthenticationError = 6028
}
