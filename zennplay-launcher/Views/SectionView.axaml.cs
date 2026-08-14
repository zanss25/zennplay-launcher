using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace zennplay_launcher.Views
{
    public partial class SectionView : UserControl
    {
        public SectionView()
        {
            InitializeComponent();
        }

        public void AddGame_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var parentWindow = TopLevel.GetTopLevel(this) as Window;

            if (parentWindow != null)
            {
                AppPropertiesWindow propertiesWindow = new();
                propertiesWindow.ShowDialog(parentWindow);
            }

        }
    }
}