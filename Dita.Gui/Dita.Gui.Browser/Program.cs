using Avalonia;
using Avalonia.Browser;
using Blazonia;
using Dita.Gui;
using System.Runtime.Versioning;
using System.Threading.Tasks;

internal sealed partial class Program
{
   /// <summary>
   /// The main entry point for the browser-based application.
   /// </summary>
   /// <param name="args">Command-line arguments.</param>
   /// <returns>A task representing the asynchronous operation.</returns>
   private static Task Main(string[] args) => BuildAvaloniaApp()
           .WithInterFont()
           .StartBrowserAppAsync("out");

   /// <summary>
   /// Configures and builds the Avalonia application instance for the browser platform.
   /// </summary>
   /// <returns>An <see cref="AppBuilder"/> configured for the browser application.</returns>
   public static AppBuilder BuildAvaloniaApp()
       => AppBuilder.Configure<App>();
}