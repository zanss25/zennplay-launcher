using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace zennplay_launcher.Views
{
    public partial class SidebarView : UserControl
    {
        public SidebarView()
        {
            InitializeComponent();
        }

        private void SettingsBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var parentWindow = TopLevel.GetTopLevel(this) as Window;

            if (parentWindow != null)
            {
                PopupView testpopup = new("Remove Game", 
                    "Are you sure you want to delete this game from your library?", // LONG TEXT PLACEHOLDER: Tapi gak ada yang namanya itu, buktinya semua hal di dunia ini adalah fana dan tidak ada yang bisa melampaui kekuasaan Tuhan?
                    "Yes", true, "No");
                testpopup.ShowDialog(parentWindow);
            }
        }
    }
}