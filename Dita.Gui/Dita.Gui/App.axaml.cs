using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dita.Gui.ViewModels;
using Dita.Gui.Views;

namespace Dita.Gui;

/// <summary>
/// The main application class for the Dita GUI application.
/// </summary>
public class App : Application
{
   /// <summary>
   /// Initializes the application by loading XAML resources.
   /// </summary>
   public override void Initialize()
   {
      AvaloniaXamlLoader.Load(this);
   }

   /// <summary>
   /// Called when the framework initialization is completed. Sets up the main window or view based on the platform.
   /// </summary>
   public override void OnFrameworkInitializationCompleted()
   {
      if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
      {
         desktop.MainWindow = new MainWindow
         {
            DataContext = new MainViewModel()
         };
      }
      else if(ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
      {
         singleViewPlatform.MainView = new MainView
         {
            DataContext = new MainViewModel()
         };
      }

      base.OnFrameworkInitializationCompleted();
   }
}