using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using zennplay_launcher.Services.Repositories;
using zennplay_launcher.Themes;
using zennplay_launcher.ViewModels;

namespace zennplay_launcher.Views
{
    public partial class AppPropertiesWindow : Window
    {
        private readonly AppPropertiesViewModel _viewModel;
        public bool IsSaved => _viewModel.IsSaved;

        public AppPropertiesWindow(Guid appId, bool isNew = false, Guid? initialSectionId = null)
        {
            InitializeComponent();
            WindowExtensions.ApplyPlatformDecorations(this);

            _viewModel = new AppPropertiesViewModel();
            _ = _viewModel.Initialize(appId, isNew, initialSectionId);
            DataContext = _viewModel;
            Title = "Edit Properties";
            TitleText.Text = Title.ToUpper();

            _viewModel.RequestClose += () => this.Close();
        }

        protected override async void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);

            if (DataContext is AppPropertiesViewModel vm)
            {
                await vm.DeleteDraftIfNotSaved();
            }

        }

        private void CancelBtn_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}