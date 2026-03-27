using Avalonia;
using Avalonia.Browser;
using Dita.Gui;
using System.Threading.Tasks;

internal sealed partial class Program
{
   /// <summary>
   /// The main entry point for the browser-based application.
   /// </summary>
   /// <returns>A task representing the asynchronous operation.</returns>
   private static Task Main() => BuildAvaloniaApp()
           .WithInterFont()
           .StartBrowserAppAsync("out");

   /// <summary>
   /// Configures and builds the Avalonia application instance for the browser platform.
   /// </summary>
   /// <returns>An <see cref="AppBuilder"/> configured for the browser application.</returns>
   public static AppBuilder BuildAvaloniaApp()
       => AppBuilder.Configure<App>();
}