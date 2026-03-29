using CommunityToolkit.Mvvm.ComponentModel;

namespace Dita.Gui.ViewModels;

/// <summary>
/// View model for the main view, providing data and logic for the main user interface.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
   /// <summary>
   /// Gets or sets the greeting message displayed in the main view.
   /// </summary>
   [ObservableProperty]
   private string _greeting = "Welcome to Avalonia!";
}