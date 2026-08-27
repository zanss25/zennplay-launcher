using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using zennplay_launcher.ViewModels.Controls;

namespace zennplay_launcher.Views.Controls
{
    public partial class PropertiesMediaView : UserControl
    {
        public PropertiesMediaView()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            if (DataContext is PropertiesMediaViewModel vm)
            {
                vm.StorageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
            }
        }

    }
}