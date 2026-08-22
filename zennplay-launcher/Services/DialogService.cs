using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Threading.Tasks;
using zennplay_launcher.Views;

namespace zennplay_launcher.Services
{
    public class DialogService : IDialogService
    {
        private readonly Window? _mainWindow;

        // 1. Constructor jika ingin passing Window spesifik
        public DialogService(Window mainWindow) => _mainWindow = mainWindow;

        // 2. Constructor default (mengambil MainWindow otomatis dari App Lifecycle)
        public DialogService()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                _mainWindow = desktop.MainWindow;
            }
        }

        private Window? GetActiveWindow()
        {
            if (_mainWindow != null) return _mainWindow;

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }

            return null;
        }

        public async Task<Guid?> ShowNewAppSelectionAsync()
        {
            var parent = GetActiveWindow();
            if (parent == null) return null;

            var win = new NewAppSelectionWindow();
            return await win.ShowDialog<Guid?>(parent);
        }

        public async Task<bool> ShowAppPropertiesAsync(Guid appId, bool isNew, Guid? sectionId)
        {
            var parent = GetActiveWindow();
            if (parent == null) return false;

            var win = new AppPropertiesWindow(appId, isNew, sectionId);
            await win.ShowDialog(parent);
            return win.IsSaved;
        }

        public static async Task<bool> ShowConfirmAsync(Window parent, string title, string message, string action = "Ok", string cancel = "Cancel")
        {
            var popup = new PopupView(title, message, action, cancel);
            return await popup.ShowDialog<bool>(parent);
        }
    }
}