using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using System.Collections;

namespace zennplay_launcher.Views.Controls
{
    public class PropertyDropdownView : TemplatedControl
    {
        // 1. Registered Property: Label
        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<PropertyDropdownView, string>(nameof(Label), string.Empty);

        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        // 2. Registered Property: ItemsSource
        public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
            AvaloniaProperty.Register<PropertyDropdownView, IEnumerable?>(nameof(ItemsSource));

        public IEnumerable? ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        // 3. Registered Property: SelectedItem
        public static readonly StyledProperty<object?> SelectedItemProperty =
            AvaloniaProperty.Register<PropertyDropdownView, object?>(
                nameof(SelectedItem),
                defaultBindingMode: BindingMode.TwoWay);

        public object? SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

    }
}