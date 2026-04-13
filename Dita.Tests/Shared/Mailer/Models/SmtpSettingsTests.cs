using Dita.Shared.Mailer.Models;
using MailKit.Security;

namespace Dita.Tests.Shared.Mailer.Models;

public class SmtpSettingsTests
{
   [Fact]
   public void WhenCreatedThenDefaultValuesAreApplied()
   {
      var settings = new SmtpSettings();

      Assert.Equal(string.Empty, settings.Host);
      Assert.Equal(578, settings.Port);
      Assert.Equal(string.Empty, settings.FromName);
      Assert.Equal(string.Empty, settings.FromMail);
      Assert.Equal(string.Empty, settings.Username);
      Assert.Equal(string.Empty, settings.Password);
      Assert.Equal(SecureSocketOptions.Auto, settings.SecureSocketOptions);
      Assert.True(settings.AuthorizationRequired);
   }

   [Fact]
   public void WhenConfiguredThenAssignedValuesPersist()
   {
      var settings = new SmtpSettings
      {
         Host = "smtp.example.com",
         Port = 587,
         FromName = "Dita",
         FromMail = "support@example.com",
         Username = "user",
         Password = "password",
         SecureSocketOptions = SecureSocketOptions.StartTls,
         AuthorizationRequired = false
      };

      Assert.Equal("smtp.example.com", settings.Host);
      Assert.Equal(587, settings.Port);
      Assert.Equal("Dita", settings.FromName);
      Assert.Equal("support@example.com", settings.FromMail);
      Assert.Equal("user", settings.Username);
      Assert.Equal("password", settings.Password);
      Assert.Equal(SecureSocketOptions.StartTls, settings.SecureSocketOptions);
      Assert.False(settings.AuthorizationRequired);
   }
}