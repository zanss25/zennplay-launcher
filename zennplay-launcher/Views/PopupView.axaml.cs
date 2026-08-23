using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using zennplay_launcher.Themes;

namespace zennplay_launcher.Views
{
    public partial class PopupView : Window
    {
        public PopupView(string title = "PopupWindow", string message = "PopupWindow: DefaultMessage",
                            string action = "Ok", bool showCancel = true, string cancel = "Cancel")
        {
            InitializeComponent();
            WindowExtensions.ApplyPlatformDecorations(this);

            TitleBar.ShowCloseButton = true;
            TitleBar.ShowMaximizeButton = false;
            TitleBar.ShowMinimizeButton = false;

            Title = title;
            TitleText.Text = Title.ToUpper();
            Message.Text = message;
            ActionBtn.Content = action;
            CancelBtn.Content = cancel;
            CancelBtn.IsVisible = showCancel;
        }

        private void CancelBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close(false);
        }

        private void ActionBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close(true);
        }

    }
}