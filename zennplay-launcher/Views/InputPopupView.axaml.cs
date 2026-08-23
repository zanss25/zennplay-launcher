using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Threading.Tasks;
using zennplay_launcher.Themes;
using zennplay_launcher.Services;

namespace zennplay_launcher.Views
{
    public partial class InputPopupView : Window
    {
        public InputPopupView(string title = "InputPopupWindow", string message = "InputPopupWindow: DefaultMessage",
                                string text = "", string action = "Ok", string cancel = "Cancel")
        {
            InitializeComponent();
            WindowExtensions.ApplyPlatformDecorations(this);

            TitleBar.ShowCloseButton = true;
            TitleBar.ShowMaximizeButton = false;
            TitleBar.ShowMinimizeButton = false;

            Title = title;
            TitleText.Text = Title.ToUpper();
            Message.Text = message;
            InputText.Text = text;
            ActionBtn.Content = action;
            CancelBtn.Content = cancel;
        }

        private void InputText_TextChanged(object? sender, TextChangedEventArgs e)
        {
            InputText.Classes.Remove("InputError");
        }

        private void CancelBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close(string.Empty);
        }

        private void ActionBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(InputText.Text))
            {
                Close(InputText.Text);
            }
            else
            {
                InputText.Classes.Add("InputError");
            }

        }

    }
}