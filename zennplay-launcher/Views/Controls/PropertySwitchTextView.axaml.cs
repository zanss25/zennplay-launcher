using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace zennplay_launcher.Views.Controls
{
    public class PropertySwitchTextView : TemplatedControl
    {
        // 1. Registered Property: Label
        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<PropertySwitchTextView, string>(nameof(Label), string.Empty);

        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        // 2. Registered Property: IsChecked (TwoWay)
        public static readonly StyledProperty<bool?> IsCheckedProperty =
            AvaloniaProperty.Register<PropertySwitchTextView, bool?>(
                nameof(IsChecked),
                defaultBindingMode: BindingMode.TwoWay);

        public bool? IsChecked
        {
            get => GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

    }
}