using Avalonia;
using Blazonia;
using System;

namespace Dita.Gui.Desktop;

internal sealed class Program
{
   // Initialization code. Don't use any Avalonia, third-party APIs or any
   // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
   // yet and stuff might break.
   
   /// <summary>
   /// The main entry point for the desktop application.
   /// </summary>
   /// <param name="args">Command-line arguments.</param>
   [STAThread]
   public static void Main(string[] args) => BuildAvaloniaApp()
       .StartWithClassicDesktopLifetime(args);

   /// <summary>
   /// Configures and builds the Avalonia application instance.
   /// </summary>
   /// <returns>An <see cref="AppBuilder"/> configured for the desktop application.</returns>
   public static AppBuilder BuildAvaloniaApp()
       => AppBuilder.Configure<App>()
           .UsePlatformDetect()
           .WithInterFont()
           .LogToTrace();
}
