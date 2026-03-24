using CommunityToolkit.Mvvm.ComponentModel;

namespace Dita.Gui.ViewModels;

public partial class MainViewModel : ViewModelBase
{
   [ObservableProperty]
   private string _greeting = "Welcome to Avalonia!";
}
