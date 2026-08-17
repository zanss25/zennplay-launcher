using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using System;

namespace zennplay_launcher.Views.Controls
{
    public class PropertyCalendarTextInputView : TemplatedControl
    {
        // 1. Registered Property: Label
        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<PropertyCalendarTextInputView, string>(nameof(Label), string.Empty);

        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        // 2. Registered Property: Text (TwoWay)
        public static readonly StyledProperty<string?> TextProperty =
            AvaloniaProperty.Register<PropertyCalendarTextInputView, string?>(
                nameof(Text),
                defaultBindingMode: BindingMode.TwoWay);

        public string? Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        // 2. Registered Property: SelectedDate (TwoWay)
        public static readonly StyledProperty<DateTime?> SelectedDateProperty =
            AvaloniaProperty.Register<PropertyCalendarTextInputView, DateTime?>(
                nameof(SelectedDate),
                defaultBindingMode: BindingMode.TwoWay);

        public DateTime? SelectedDate
        {
            get => GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        // 3. Registered Property: PlaceholderText
        public static readonly StyledProperty<string> PlaceholderTextProperty =
            AvaloniaProperty.Register<PropertyInputView, string>(nameof(PlaceholderText), string.Empty);

        public string PlaceholderText
        {
            get => GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

    }
}