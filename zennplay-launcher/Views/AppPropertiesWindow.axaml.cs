using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using zennplay_launcher.ViewModels;

namespace zennplay_launcher.Views
{
    public partial class AppPropertiesWindow : Window
    {
        private readonly AppPropertiesViewModel _viewModel;

        public AppPropertiesWindow(int appId = -1)
        {
            InitializeComponent();

            _viewModel = new AppPropertiesViewModel();
            _viewModel.Initialize(appId);
            DataContext = _viewModel;

            if (appId < 0)
            {
                Title = "Add New Properties";
            }
            else
            {
                Title = "Edit Properties";
            }

            TitleText.Text = Title.ToUpper();
        }

        private void SaveBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {

        }

        private void CancelBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }
    }
}