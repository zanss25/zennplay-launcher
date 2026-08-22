using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace zennplay_launcher.Views;

public partial class TitleBarView : UserControl
{
    // Styled Properties untuk kontrol visibilitas
    public static readonly StyledProperty<bool> ShowMinimizeButtonProperty =
        AvaloniaProperty.Register<TitleBarView, bool>(nameof(ShowMinimizeButton), defaultValue: true);

    public static readonly StyledProperty<bool> ShowMaximizeButtonProperty =
        AvaloniaProperty.Register<TitleBarView, bool>(nameof(ShowMaximizeButton), defaultValue: true);

    public static readonly StyledProperty<bool> ShowCloseButtonProperty =
        AvaloniaProperty.Register<TitleBarView, bool>(nameof(ShowCloseButton), defaultValue: true);

    public static readonly StyledProperty<bool> ShowButtonsProperty =
        AvaloniaProperty.Register<TitleBarView, bool>(nameof(ShowButtons), defaultValue: true);

    public bool ShowMinimizeButton
    {
        get => GetValue(ShowMinimizeButtonProperty);
        set => SetValue(ShowMinimizeButtonProperty, value);
    }

    public bool ShowMaximizeButton
    {
        get => GetValue(ShowMaximizeButtonProperty);
        set => SetValue(ShowMaximizeButtonProperty, value);
    }

    public bool ShowCloseButton
    {
        get => GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    public bool ShowButtons
    {
        get => GetValue(ShowButtonsProperty);
        set => SetValue(ShowButtonsProperty, value);
    }

    public TitleBarView()
    {
        InitializeComponent();
    }

    private Window? GetParentWindow() => TopLevel.GetTopLevel(this) as Window;

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            GetParentWindow()?.BeginMoveDrag(e);
        }
    }

    private void OnTitleBarDoubleTapped(object? sender, TappedEventArgs e)
    {
        var window = GetParentWindow();
        if (window != null && ShowMaximizeButton)
        {
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
    }

    private void MinimizeBtn_Click(object? sender, RoutedEventArgs e)
    {
        var window = GetParentWindow();
        if (window != null)
            window.WindowState = WindowState.Minimized;
    }

    private void MaximizeBtn_Click(object? sender, RoutedEventArgs e)
    {
        var window = GetParentWindow();
        if (window != null)
        {
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
    }

    private void CloseBtn_Click(object? sender, RoutedEventArgs e)
    {
        GetParentWindow()?.Close();
    }
}