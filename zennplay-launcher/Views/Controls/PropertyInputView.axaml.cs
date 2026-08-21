using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace zennplay_launcher.Views.Controls;

public class PropertyInputView : TemplatedControl
{
    // 1. Registered Property: Label
    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<PropertyInputView, string>(nameof(Label), string.Empty);

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // 2. Registered Property: Text (TwoWay)
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<PropertyInputView, string?>(
            nameof(Text),
            defaultBindingMode: BindingMode.TwoWay);

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    // 3. Registered Property: PlaceholderText
    public static readonly StyledProperty<string> PlaceholderTextProperty =
        AvaloniaProperty.Register<PropertyInputView, string>(nameof(PlaceholderText), string.Empty);

    public string PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    // 4. Registered Property: MinHeight (TwoWay)
    public static readonly StyledProperty<double> InputMinHeightProperty =
        AvaloniaProperty.Register<PropertyInputView, double>(
            nameof(InputMinHeight),
            defaultValue: 0.0);

    public double InputMinHeight
    {
        get => GetValue(InputMinHeightProperty);
        set => SetValue(InputMinHeightProperty, value);
    }

}