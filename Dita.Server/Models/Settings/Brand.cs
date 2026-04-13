namespace Dita.Server.Models.Settings;

/// <summary>
/// The class representing the brand information for the application, including the name, logo URL, and support email.
/// This information can be used to customize the appearance and contact details of the application for different brands
/// or clients.
/// </summary>
public class Brand
{
   /// <summary>
   /// Gets or sets the name associated with this instance.
   /// </summary>
   public string Name { get; set; } = "Afrowave";

   /// <summary>
   /// Gets or sets the URL of the logo image associated with this instance.
   /// </summary>
   public string LogoUrl { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the support email address for contacting customer service or technical support.
   /// </summary>
   public string SupportEmail { get; set; } = string.Empty;
}