using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using zennplay_launcher.ViewModels.Controls;

namespace zennplay_launcher.Views.Controls
{
    public partial class PropertiesFoldersView : UserControl
    {
        public PropertiesFoldersView()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            if (DataContext is PropertiesFoldersViewModel vm)
            {
                vm.StorageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
            }
        }

    }
}