using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using zennplay_launcher.ViewModels;

namespace zennplay_launcher.Views
{
    public partial class AppPropertiesWindow : Window
    {
        private readonly AppPropertiesViewModel _viewModel;

        public AppPropertiesWindow(Guid appId = default)
        {
            InitializeComponent();

            _viewModel = new AppPropertiesViewModel();
            _viewModel.Initialize(appId);
            DataContext = _viewModel;

            if (appId == Guid.Empty)
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