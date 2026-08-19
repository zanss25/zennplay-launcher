using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
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

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            if (DataContext is AppPropertiesViewModel mainVm)
            {
                if (mainVm.FoldersViewModel != null)
                {
                    mainVm.FoldersViewModel.StorageProvider = this.StorageProvider;
                }
            }
        }

    }
}