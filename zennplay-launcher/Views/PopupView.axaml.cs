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