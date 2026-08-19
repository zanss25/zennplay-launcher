using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace zennplay_launcher.Views.Controls
{
    public class PropertyDirectoryInputView : TemplatedControl
    {
        // 1. Registered Property: Label
        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<PropertyDirectoryInputView, string>(nameof(Label), string.Empty);

        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        // 2. Registered Property: Text (TwoWay)
        public static readonly StyledProperty<string?> TextProperty =
            AvaloniaProperty.Register<PropertyDirectoryInputView, string?>(
                nameof(Text),
                defaultBindingMode: BindingMode.TwoWay);

        public string? Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        // 3. Registered Property: PlaceholderText
        public static readonly StyledProperty<string> PlaceholderTextProperty =
            AvaloniaProperty.Register<PropertyDirectoryInputView, string>(nameof(PlaceholderText), string.Empty);

        public string PlaceholderText
        {
            get => GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }
    }
}