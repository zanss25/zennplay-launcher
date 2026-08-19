using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using System.Collections;
using System.Windows.Input;

namespace zennplay_launcher.Views.Controls;

public class EditableListView : TemplatedControl
{
    // ItemsSource
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<EditableListView, IEnumerable?>(nameof(ItemsSource));

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    // SelectedItem (Single Selection)
    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<EditableListView, object?>(
            nameof(SelectedItem),
            defaultBindingMode: BindingMode.TwoWay);

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    // Dynamic Action Button Text (Default: "Edit")
    public static readonly StyledProperty<string> ActionButtonTextProperty =
        AvaloniaProperty.Register<EditableListView, string>(nameof(ActionButtonText), "Edit");

    public string ActionButtonText
    {
        get => GetValue(ActionButtonTextProperty);
        set => SetValue(ActionButtonTextProperty, value);
    }

    // Commands
    public static readonly StyledProperty<ICommand?> AddCommandProperty =
        AvaloniaProperty.Register<EditableListView, ICommand?>(nameof(AddCommand));
    public ICommand? AddCommand
    {
        get => GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand?> RemoveCommandProperty =
        AvaloniaProperty.Register<EditableListView, ICommand?>(nameof(RemoveCommand));
    public ICommand? RemoveCommand
    {
        get => GetValue(RemoveCommandProperty);
        set => SetValue(RemoveCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand?> ActionCommandProperty =
        AvaloniaProperty.Register<EditableListView, ICommand?>(nameof(ActionCommand));
    public ICommand? ActionCommand
    {
        get => GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand?> MoveUpCommandProperty =
        AvaloniaProperty.Register<EditableListView, ICommand?>(nameof(MoveUpCommand));
    public ICommand? MoveUpCommand
    {
        get => GetValue(MoveUpCommandProperty);
        set => SetValue(MoveUpCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand?> MoveDownCommandProperty =
        AvaloniaProperty.Register<EditableListView, ICommand?>(nameof(MoveDownCommand));
    public ICommand? MoveDownCommand
    {
        get => GetValue(MoveDownCommandProperty);
        set => SetValue(MoveDownCommandProperty, value);
    }
}