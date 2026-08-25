using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using zennplay_launcher.Themes;

namespace zennplay_launcher.Views
{
    public partial class AppActionPropertiesWindow : Window
    {
        public AppActionPropertiesWindow()
        {
            InitializeComponent();
            WindowExtensions.ApplyPlatformDecorations(this);

            Title = "App Action Properties";
            TitleText.Text = Title.ToUpper();
        }

    }
}