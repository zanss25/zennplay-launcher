using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace zennplay_launcher.Views.Controls
{
    public partial class PropertiesMetadataView : UserControl
    {
        public PropertiesMetadataView()
        {
            InitializeComponent();

            IsNotGameSwitch.IsChecked = false;
            IsExcludedSwitch.IsChecked = false;
            IsPinnedSwitch.IsChecked = false;
            
        }
    }
}