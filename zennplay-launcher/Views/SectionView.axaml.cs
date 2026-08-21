using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using zennplay_launcher.ViewModels;

namespace zennplay_launcher.Views
{
    public partial class SectionView : UserControl
    {
        public SectionView()
        {
            InitializeComponent();
        }

        public async void AddGame_Click(object? sender, RoutedEventArgs e)
        {
            var parentWindow = TopLevel.GetTopLevel(this) as Window;
            if (parentWindow == null) return;

            // 1. Buka dialog selection dan TUNGGU hasilnya (mengembalikan Guid? yang dikirim via this.Close(...))
            var newAppWin = new NewAppSelectionWindow();
            Guid? resultAppId = await newAppWin.ShowDialog<Guid?>(parentWindow);

            // 2. Jika user membatalkan (Cancel), proses berhenti di sini
            if (!resultAppId.HasValue || resultAppId.Value == Guid.Empty)
            {
                return;
            }

            // 3. Jika berhasil, buat dan tampilkan AppPropertiesWindow dengan parentWindow yang sama
            var propertiesVm = new AppPropertiesViewModel();
            propertiesVm.Initialize(resultAppId.Value, isNew: true);

            var propertiesWindow = new AppPropertiesWindow(resultAppId.Value)
            {
                DataContext = propertiesVm
            };

            // Buka properties secara modal dialog atau show biasa terhadap parentWindow
            await propertiesWindow.ShowDialog(parentWindow);
        }

    }
}