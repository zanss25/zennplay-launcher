using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using zennplay_launcher.Models;
using zennplay_launcher.Services;
using zennplay_launcher.Services.Repositories;
using zennplay_launcher.Themes;

namespace zennplay_launcher.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var repo = new SectionRepository();

        //repo.AddSection(new SectionModel { Name = "Modding", Desc = "Modding Tools", Icon = "console" });
        //repo.AddSection(new SectionModel { Name = "Single Player", Desc = "Single Player Games", Icon = "gamepad" });
        //repo.AddSection(new SectionModel { Name = "Multiplayer", Desc = "Multiplayer Games", Icon = "tool" });
        
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            // Memulai fungsi Drag bawaan OS
            BeginMoveDrag(e);
        }
    }

    private void OnTitleBarDoubleTapped(object? sender, TappedEventArgs e)
    {
        this.WindowState = this.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void MinimizeBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void MaximizeBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.WindowState = this.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void CloseBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}