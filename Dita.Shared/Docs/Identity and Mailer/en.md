# Identity and Mailer

The `Dita.Shared.Identity` and `Dita.Shared.Mailer` namespaces provide foundational models for authentication and email infrastructure.

## Identity

### LoginResponse enum

Namespace: `Dita.Shared.Identity.Enums`

Defines all possible outcomes of a login attempt. Rather than throwing exceptions or returning booleans, the authentication system returns a `LoginResponse` value, enabling the caller to branch on the specific failure case and provide appropriate user feedback.

| Value | Name | Description |
|---|---|---|
| 0 | `Success` | Login was successful |
| 1 | `InvalidCredentials` | The provided username or password was invalid |
| 2 | `UserNotFound` | No account matching the provided identifier was found |
| 3 | `LockedOut` | The account is temporarily locked due to too many failed attempts |
| 4 | `Banned` | The account has been permanently banned |
| 5 | `TwoFactorRequired` | Two-factor authentication is required to complete login |
| 6 | `UnknownError` | An unexpected error occurred |

### Usage pattern

```csharp
LoginResponse result = authService.Login(username, password);

return result switch
{
    LoginResponse.Success => RedirectToAction("Dashboard"),
    LoginResponse.InvalidCredentials => View("InvalidCredentials"),
    LoginResponse.LockedOut => View("AccountLocked"),
    LoginResponse.TwoFactorRequired => RedirectToAction("TwoFactor"),
    _ => View("LoginError")
};
```

### Design patterns

- **Result pattern / Discriminated-union style** — encodes every distinct failure case as a named value, making all failure paths explicit and exhaustive in `switch` expressions
- **Separation of concerns** — authentication domain primitives live in `Dita.Shared.Identity.Enums`, isolated from business logic and infrastructure

---

## Mailer

### SmtpSettings model

Namespace: `Dita.Shared.Mailer.Models`

Configuration model for SMTP email sending, bound from `appsettings.json`.

| Property | Type | Default | Description |
|---|---|---|---|
| `Host` | `string` | `""` | SMTP server hostname |
| `Port` | `int` | `578` | SMTP server port |
| `FromName` | `string` | `""` | Sender display name |
| `FromMail` | `string` | `""` | Sender email address |
| `Username` | `string` | `""` | SMTP authentication username |
| `Password` | `string` | `""` | SMTP authentication password |
| `SecureSocketOptions` | `SecureSocketOptions` | `Auto` | MailKit socket security (Auto, SslOnConnect, StartTls, None) |
| `AuthorizationRequired` | `bool` | `true` | Whether SMTP authentication is required |

### Planned services

The `Mailer/Services/` directory is explicitly declared in the `.csproj` but currently empty. Based on the **MailKit 4.16.0** NuGet dependency, a future `IMailerService` / `MailerService` is expected to provide:

- Email sending with SMTP configuration from `SmtpSettings`
- HTML email body rendering (suggested by the **Markdig** dependency — Markdown-to-HTML conversion for email templates)
- Template-based email composition with localization support

### Configuration example

```json
{
  "SmtpSettings": {
    "Host": "smtp.example.com",
    "Port": 587,
    "FromName": "Dita Server",
    "FromMail": "noreply@example.com",
    "Username": "api-key",
    "Password": "smtp-password",
    "SecureSocketOptions": "StartTls",
    "AuthorizationRequired": true
  }
}
```

> **Note:** The default port is `578`, which appears to be a typo for the standard SMTP submission port `587`. This should be overridden in production configuration.