using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using System;
using System.Windows.Input;

namespace zennplay_launcher.Views.Controls
{
    public class DirectoryInputView : TemplatedControl
    {
        private TextBox? _textBox;
        private Button? _browseButton;

        // --- STYLED PROPERTIES ---

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

        // 3. Registered Property: Browse Button
        public static readonly StyledProperty<ICommand?> BrowseCommandProperty =
            AvaloniaProperty.Register<DirectoryInputView, ICommand?>(nameof(BrowseCommand));

        public ICommand? BrowseCommand
        {
            get => GetValue(BrowseCommandProperty);
            set => SetValue(BrowseCommandProperty, value);
        }

        public event EventHandler? BrowseRequested;

        // --- OVERRIDES & TEMPLATE LOGIC ---

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (_browseButton != null)
                _browseButton.Click -= OnBrowseButtonClicked;

            _textBox = e.NameScope.Find<TextBox>("PART_TextBox");
            _browseButton = e.NameScope.Find<Button>("PART_BrowseButton");

            if (_browseButton != null)
                _browseButton.Click += OnBrowseButtonClicked;
        }

        private void OnBrowseButtonClicked(object? sender, RoutedEventArgs e)
        {
            BrowseRequested?.Invoke(this, EventArgs.Empty);

            if (BrowseCommand != null && BrowseCommand.CanExecute(null))
            {
                BrowseCommand.Execute(null);
            }
        }

    }
}