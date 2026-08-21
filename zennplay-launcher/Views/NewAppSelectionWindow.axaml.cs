using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using zennplay_launcher.ViewModels;

namespace zennplay_launcher.Views
{
    public partial class NewAppSelectionWindow : Window
    {
        public NewAppSelectionWindow()
        {
            InitializeComponent();

            DataContext = new NewAppSelectionViewModel();
        }

        protected override async void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            if (DataContext is NewAppSelectionViewModel vm)
            {
                Guid? newAppId = await vm.ManualAppBrowseAsync(this.StorageProvider);

                // Jika user pilih file -> mengembalikan Guid
                // Jika cancel / invalid -> mengembalikan null
                this.Close(newAppId);
            }
        }

    }
}