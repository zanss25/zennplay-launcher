using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace zennplay_launcher.Views.Controls
{
    public partial class WorkInProgressView : UserControl
    {
        public WorkInProgressView()
        {
            InitializeComponent();

            WipInfo.BannerText.Text = "COMING SOON";
        }
    }
}