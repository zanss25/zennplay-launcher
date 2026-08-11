using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace zennplay_launcher.Views;

public partial class TitleBarView : UserControl
{
    public TitleBarView()
    {
        InitializeComponent();
    }

    // Helper method untuk mengambil Window induk secara aman
    private Window? GetParentWindow()
    {
        return TopLevel.GetTopLevel(this) as Window;
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            // Panggil BeginMoveDrag dari instance Window
            GetParentWindow()?.BeginMoveDrag(e);
        }
    }

    private void OnTitleBarDoubleTapped(object? sender, TappedEventArgs e)
    {
        var window = GetParentWindow();
        if (window != null)
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
        {
            window.WindowState = WindowState.Minimized;
        }
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