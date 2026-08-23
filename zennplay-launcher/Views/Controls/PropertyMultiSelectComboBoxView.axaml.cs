using System;
using System.Collections;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace zennplay_launcher.Views.Controls
{
    public class PropertyMultiSelectComboBoxView : TemplatedControl
    {
        protected override Type StyleKeyOverride => typeof(PropertyMultiSelectComboBoxView);

        // 1. Label Teks di Kolom Kiri
        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<PropertyMultiSelectComboBoxView, string>(nameof(Label), string.Empty);

        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        // 2. Contents (Dua Arah / TwoWay Binding ke ViewModel)
        public static readonly StyledProperty<string?> ContentsProperty =
            AvaloniaProperty.Register<PropertyMultiSelectComboBoxView, string?>(
                nameof(Contents),
                defaultBindingMode: BindingMode.TwoWay);

        public string? Contents
        {
            get => GetValue(ContentsProperty);
            set => SetValue(ContentsProperty, value);
        }

        // 3. Placeholder / Watermark
        public static readonly StyledProperty<string> PlaceholderTextProperty =
            AvaloniaProperty.Register<PropertyMultiSelectComboBoxView, string>(nameof(PlaceholderText), string.Empty);

        public string PlaceholderText
        {
            get => GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        // 4. Data Master Items (Genre/Tags dari SQLite)
        public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
            AvaloniaProperty.Register<PropertyMultiSelectComboBoxView, IEnumerable?>(nameof(ItemsSource));

        public IEnumerable? ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        // 5. Teks pada Tombol New (misal: "New Genre", "New Tag")
        public static readonly StyledProperty<string> AddNewTextProperty =
            AvaloniaProperty.Register<PropertyMultiSelectComboBoxView, string>(nameof(AddNewText), "New Item");

        public string AddNewText
        {
            get => GetValue(AddNewTextProperty);
            set => SetValue(AddNewTextProperty, value);
        }

        // 6. Command Tombol New (+ New Dialog)
        public static readonly StyledProperty<ICommand?> AddNewCommandProperty =
            AvaloniaProperty.Register<PropertyMultiSelectComboBoxView, ICommand?>(nameof(AddNewCommand));

        public ICommand? AddNewCommand
        {
            get => GetValue(AddNewCommandProperty);
            set => SetValue(AddNewCommandProperty, value);
        }

        public event EventHandler<string>? ItemAdded;

    }
}