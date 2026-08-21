using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace zennplay_launcher.Views.Controls
{
    public class PropertyLongInputView : TemplatedControl
    {
        // 1. Registered Property: Label
        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<PropertyLongInputView, string>(nameof(Label), string.Empty);

        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        // 2. Registered Property: Text (TwoWay)
        public static readonly StyledProperty<string?> TextProperty =
            AvaloniaProperty.Register<PropertyLongInputView, string?>(
                nameof(Text),
                defaultBindingMode: BindingMode.TwoWay);

        public string? Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        // 3. Registered Property: PlaceholderText
        public static readonly StyledProperty<string> PlaceholderTextProperty =
            AvaloniaProperty.Register<PropertyLongInputView, string>(nameof(PlaceholderText), string.Empty);

        public string PlaceholderText
        {
            get => GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        // 4. Registered Property: MinHeight (TwoWay)
        public static readonly StyledProperty<double> InputMinHeightProperty =
            AvaloniaProperty.Register<PropertyLongInputView, double>(
                nameof(InputMinHeight),
                defaultValue: 0.0);

        public double InputMinHeight
        {
            get => GetValue(InputMinHeightProperty);
            set => SetValue(InputMinHeightProperty, value);
        }


    }
}