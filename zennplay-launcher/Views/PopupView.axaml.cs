using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace zennplay_launcher.Views
{
    public partial class PopupView : Window
    {
        public PopupView(string title = "PopupWindow", string message = "PopupWindow: DefaultMessage",
                            string action = "Ok", string cancel = "Cancel")
        {
            InitializeComponent();

            Title = title;
            TitleText.Text = Title.ToUpper();
            Message.Text = message;
            ActionBtn.Content = action;
            CancelBtn.Content = cancel;
        }

        private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                // Memulai fungsi Drag bawaan OS
                BeginMoveDrag(e);
            }
        }

        private void OnTitleBarDoubleTapped(object? sender, TappedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void MinimizeBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void CloseBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }

        private void CancelBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }

        private void ActionBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }
    }
}