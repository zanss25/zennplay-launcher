using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using zennplay_launcher.Themes;

namespace zennplay_launcher.Views
{
    public partial class AppLinkPropertiesWindow : Window
    {
        public AppLinkPropertiesWindow()
        {
            InitializeComponent();

            WindowExtensions.ApplyPlatformDecorations(this);

            TitleBar.ShowCloseButton = true;
            TitleBar.ShowMaximizeButton = false;
            TitleBar.ShowMinimizeButton = false;

            Title = "App Link Properties";
            TitleText.Text = Title.ToUpper();
        }

    }
}
