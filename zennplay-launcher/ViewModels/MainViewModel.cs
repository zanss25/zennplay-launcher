using CommunityToolkit.Mvvm.ComponentModel;

namespace zennplay_launcher.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _zenn_logo_text = "ZENN";
}
