using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace zennplay_launcher.Views.Controls
{
    public class DirectoryInputView : TemplatedControl
    {
        // 1. Registered Property: Text (TwoWay)
        public static readonly StyledProperty<string?> TextProperty =
            AvaloniaProperty.Register<DirectoryInputView, string?>(
                nameof(Text),
                defaultBindingMode: BindingMode.TwoWay);

        public string? Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        // 2. Registered Property: PlaceholderText
        public static readonly StyledProperty<string> PlaceholderTextProperty =
            AvaloniaProperty.Register<DirectoryInputView, string>(nameof(PlaceholderText), string.Empty);

        public string PlaceholderText
        {
            get => GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

    }
}