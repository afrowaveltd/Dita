using MailKit.Security;

namespace Dita.Shared.Mailer.Models;
/// <summary>
/// Represents the SMTP settings required to configure an email client for sending emails.
/// </summary>
public class SmtpSettings
{
   /// <summary>
   /// Gets or sets the SMTP server host address. This is the address of the email server that will be used to send emails.
   /// </summary>
   public string Host { get; set; } = string.Empty;
   /// <summary>
   /// Gets or sets the SMTP server port number. This is the port on which the email server is listening for incoming connections. The default value is 578, but it can be changed based on the server configuration.
   /// </summary>
   public int Port { get; set; } = 578;
   /// <summary>
   /// Gets or sets the display name of the sender. This is the name that will appear in the "From" field of the email when recipients receive it.
   /// </summary>
   public string FromName { get; set; } = string.Empty;
   /// <summary>
   /// Gets or sets the email address of the sender. This is the email address that will appear in the "From" field of the email when recipients receive it. It should be a valid email address format.
   /// </summary>
   public string FromMail { get; set; } = string.Empty;
   /// <summary>
   /// Gets or sets the username for SMTP authentication. This is the username that will be used to authenticate with the SMTP server when sending emails. It is required if the SMTP server requires authentication.
   /// </summary>
   public string Username { get; set; } = string.Empty;
   /// <summary>
   /// Gets or sets the password for SMTP authentication. This is the password that will be used to authenticate with the SMTP server when sending emails. It is required if the SMTP server requires authentication.
   /// </summary>
   public string Password { get; set; } = string.Empty;
   /// <summary>
   /// Gets or sets the secure socket options for the SMTP connection. This property determines how the email client will establish a secure connection with the SMTP server. The default value is `SecureSocketOptions.Auto`, which means that the client will automatically determine the appropriate security options based on the server's capabilities. Other options include `SecureSocketOptions.SslOnConnect` for SSL/TLS encryption, `SecureSocketOptions.StartTls` for STARTTLS encryption, and `SecureSocketOptions.None` for no encryption.
   /// </summary>
   public SecureSocketOptions SecureSocketOptions { get; set; } = SecureSocketOptions.Auto;
   /// <summary>
   /// Gets or sets a value indicating whether SMTP authentication is required. This property specifies whether the email client needs to authenticate with the SMTP server using the provided username and password before sending emails. The default value is `true`, which means that authentication is required. If set to `false`, the email client will attempt to send emails without authentication, which may be allowed by some SMTP servers depending on their configuration.
   /// </summary>
   public bool AuthorizationRequired { get; set; } = true;
}
