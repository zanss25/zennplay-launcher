using Avalonia;
using Avalonia.Controls.Primitives;

namespace zennplay_launcher.Views.Controls
{
    public class OptionHeaderView : TemplatedControl
    {
        // 1. Registered Property: Label
        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<PropertyInputView, string>(nameof(Label), string.Empty);

        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }
    }
}