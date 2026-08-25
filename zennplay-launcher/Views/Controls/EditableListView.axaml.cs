using System;
using System.Collections;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using zennplay_launcher.Models;

namespace zennplay_launcher.Views.Controls;

public class EditableListView : TemplatedControl
{
    private ListBox? _listBox;
    private Border? _ghostOverlay;
    private TextBlock? _ghostText;

    private Point _dragStartPoint;
    private object? _draggedItem;
    private bool _isDragging;

    protected override Type StyleKeyOverride => typeof(EditableListView);

    // ItemsSource
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<EditableListView, IEnumerable?>(nameof(ItemsSource));

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    // SelectedItem
    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<EditableListView, object?>(
            nameof(SelectedItem),
            defaultBindingMode: BindingMode.TwoWay);

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    // Commands
    public static readonly StyledProperty<ICommand?> AddCommandProperty =
        AvaloniaProperty.Register<EditableListView, ICommand?>(nameof(AddCommand));
    public ICommand? AddCommand
    {
        get => GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand?> EditCommandProperty =
        AvaloniaProperty.Register<EditableListView, ICommand?>(nameof(EditCommand));
    public ICommand? EditCommand
    {
        get => GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand?> RemoveCommandProperty =
        AvaloniaProperty.Register<EditableListView, ICommand?>(nameof(RemoveCommand));
    public ICommand? RemoveCommand
    {
        get => GetValue(RemoveCommandProperty);
        set => SetValue(RemoveCommandProperty, value);
    }

    public static readonly StyledProperty<string?> AddTextProperty =
        AvaloniaProperty.Register<PropertyInputView, string?>(
            nameof(AddText),
            defaultBindingMode: BindingMode.TwoWay);

    public string? AddText
    {
        get => GetValue(AddTextProperty);
        set => SetValue(AddTextProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_listBox != null)
        {
            _listBox.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
            _listBox.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
            _listBox.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
            _listBox.RemoveHandler(InputElement.PointerCaptureLostEvent, OnPointerCaptureLost);
        }

        _listBox = e.NameScope.Find<ListBox>("PART_ListBox");
        _ghostOverlay = e.NameScope.Find<Border>("PART_GhostOverlay");
        _ghostText = e.NameScope.Find<TextBlock>("PART_GhostText");

        if (_listBox != null)
        {
            _listBox.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
            _listBox.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);
            _listBox.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
            _listBox.AddHandler(InputElement.PointerCaptureLostEvent, OnPointerCaptureLost, RoutingStrategies.Tunnel);
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(this).Properties;
        if (properties.IsLeftButtonPressed)
        {
            _dragStartPoint = e.GetPosition(this);
            var item = (e.Source as Visual)?.FindAncestorOfType<ListBoxItem>();
            _draggedItem = item?.DataContext;
            _isDragging = false;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var properties = e.GetCurrentPoint(this).Properties;
        if (_draggedItem == null || !properties.IsLeftButtonPressed || _listBox == null)
            return;

        var currentPoint = e.GetPosition(this);
        var diff = _dragStartPoint - currentPoint;

        // 1. Aktifkan mode drag jika melebihi threshold
        if (!_isDragging && (Math.Abs(diff.X) > 6 || Math.Abs(diff.Y) > 6))
        {
            _isDragging = true;
            e.Pointer.Capture(_listBox);

            if (_ghostOverlay != null && _ghostText != null)
            {
                if (_draggedItem is IEditableListItem editable)
                    _ghostText.Text = editable.DisplayName;

                _ghostOverlay.IsVisible = true;
            }
        }

        // 2. Geser posisi ghost card mengikuti kursor
        if (_isDragging)
        {
            if (_ghostOverlay != null)
            {
                var ghostPos = e.GetPosition(_listBox);
                Canvas.SetLeft(_ghostOverlay, ghostPos.X + 12);
                Canvas.SetTop(_ghostOverlay, ghostPos.Y + 8);
            }

            // 3. Deteksi baris yang dilewati untuk reordering
            var positionInListBox = e.GetPosition(_listBox);
            var hitVisual = _listBox.InputHitTest(positionInListBox) as Visual;
            var targetContainer = hitVisual?.FindAncestorOfType<ListBoxItem>();
            var targetItem = targetContainer?.DataContext;

            if (targetItem != null && _draggedItem != null && !ReferenceEquals(_draggedItem, targetItem))
            {
                if (ItemsSource is IList list)
                {
                    int oldIndex = list.IndexOf(_draggedItem);
                    int newIndex = list.IndexOf(targetItem);

                    if (oldIndex >= 0 && newIndex >= 0)
                    {
                        list.RemoveAt(oldIndex);
                        list.Insert(newIndex, _draggedItem);
                        SelectedItem = _draggedItem;
                    }
                }
            }
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e) => ResetDrag(e.Pointer);
    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e) => ResetDrag(e.Pointer);

    private void ResetDrag(IPointer pointer)
    {
        if (_isDragging)
        {
            pointer.Capture(null);
            _isDragging = false;
        }

        if (_ghostOverlay != null)
        {
            _ghostOverlay.IsVisible = false;
        }

        _draggedItem = null;
    }
}