using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace zennplay_launcher.Views.Tabs
{
    public partial class WorkInProgress : UserControl
    {
        public WorkInProgress()
        {
            InitializeComponent();

            WipInfo.BannerText.Text = "COMING SOON";
        }
    }
}