using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using zennplay_launcher.Models;

namespace zennplay_launcher.Views.Controls;

public class MultiSelectComboBox : TemplatedControl
{
    private TextBox? _textBox;
    private Button? _addNewButton;
    private TextBox? _newItemTextBox;
    private Button? _confirmAddButton;
    private Button? _cancelAddButton;

    private bool _isUpdatingTextInternally;
    private bool _isUpdatingSelectionInternally;

    // --- STYLED & DIRECT PROPERTIES ---

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<MultiSelectComboBox, string?>(
            nameof(Text),
            defaultBindingMode: BindingMode.TwoWay);

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<string> PlaceholderTextProperty =
        AvaloniaProperty.Register<MultiSelectComboBox, string>(nameof(PlaceholderText), string.Empty);

    public string PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public static readonly StyledProperty<string> AddNewTextProperty =
        AvaloniaProperty.Register<MultiSelectComboBox, string>(nameof(AddNewText), "New Item");

    public string AddNewText
    {
        get => GetValue(AddNewTextProperty);
        set => SetValue(AddNewTextProperty, value);
    }

    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<MultiSelectComboBox, bool>(
            nameof(IsDropDownOpen),
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public static readonly StyledProperty<bool> IsAddingNewProperty =
        AvaloniaProperty.Register<MultiSelectComboBox, bool>(
            nameof(IsAddingNew),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsAddingNew
    {
        get => GetValue(IsAddingNewProperty);
        set => SetValue(IsAddingNewProperty, value);
    }

    public static readonly StyledProperty<double> MaxDropDownHeightProperty =
        AvaloniaProperty.Register<MultiSelectComboBox, double>(nameof(MaxDropDownHeight), 300.0);

    public double MaxDropDownHeight
    {
        get => GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<MultiSelectComboBox, IEnumerable?>(nameof(ItemsSource));

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DirectProperty<MultiSelectComboBox, ObservableCollection<SelectableItem>> FilteredItemsProperty =
        AvaloniaProperty.RegisterDirect<MultiSelectComboBox, ObservableCollection<SelectableItem>>(
            nameof(FilteredItems), o => o.FilteredItems);

    private ObservableCollection<SelectableItem> _filteredItems = new();
    public ObservableCollection<SelectableItem> FilteredItems
    {
        get => _filteredItems;
        private set => SetAndRaise(FilteredItemsProperty, ref _filteredItems, value);
    }

    public static readonly StyledProperty<ICommand?> AddNewCommandProperty =
        AvaloniaProperty.Register<MultiSelectComboBox, ICommand?>(nameof(AddNewCommand));

    public ICommand? AddNewCommand
    {
        get => GetValue(AddNewCommandProperty);
        set => SetValue(AddNewCommandProperty, value);
    }

    public event EventHandler<string>? ItemAdded;

    // --- TEMPLATE & LIFECYCLE ---

    protected override Type StyleKeyOverride => typeof(MultiSelectComboBox);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_addNewButton != null) _addNewButton.Click -= OnAddNewButtonClicked;
        if (_confirmAddButton != null) _confirmAddButton.Click -= OnConfirmAddClicked;
        if (_cancelAddButton != null) _cancelAddButton.Click -= OnCancelAddClicked;
        if (_newItemTextBox != null) _newItemTextBox.KeyDown -= OnNewItemTextBoxKeyDown;

        _textBox = e.NameScope.Find<TextBox>("PART_TextBox");
        _addNewButton = e.NameScope.Find<Button>("PART_AddNewButton");
        _newItemTextBox = e.NameScope.Find<TextBox>("PART_NewItemTextBox");
        _confirmAddButton = e.NameScope.Find<Button>("PART_ConfirmAddButton");
        _cancelAddButton = e.NameScope.Find<Button>("PART_CancelAddButton");

        if (_addNewButton != null) _addNewButton.Click += OnAddNewButtonClicked;
        if (_confirmAddButton != null) _confirmAddButton.Click += OnConfirmAddClicked;
        if (_cancelAddButton != null) _cancelAddButton.Click += OnCancelAddClicked;
        if (_newItemTextBox != null) _newItemTextBox.KeyDown += OnNewItemTextBoxKeyDown;

        SyncFilteredItems();
        SyncSelectionFromText();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            UnsubscribeFromItems(change.OldValue as IEnumerable);
            SubscribeToItems(change.NewValue as IEnumerable);
            SyncFilteredItems();
            SyncSelectionFromText();
        }
        else if (change.Property == TextProperty)
        {
            if (!_isUpdatingTextInternally)
            {
                SyncSelectionFromText();
            }
        }
        else if (change.Property == IsDropDownOpenProperty && !change.GetNewValue<bool>())
        {
            CancelAddNew();
        }
    }

    // --- INLINE ADD HANDLERS ---

    private void OnAddNewButtonClicked(object? sender, RoutedEventArgs e)
    {
        IsAddingNew = true;
        if (_newItemTextBox != null)
        {
            _newItemTextBox.Text = string.Empty;
            Dispatcher.UIThread.Post(() => _newItemTextBox.Focus());
        }
    }

    private void OnConfirmAddClicked(object? sender, RoutedEventArgs e) => ConfirmAddNew();
    private void OnCancelAddClicked(object? sender, RoutedEventArgs e) => CancelAddNew();

    private void OnNewItemTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ConfirmAddNew();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            CancelAddNew();
            e.Handled = true;
        }
    }

    private void ConfirmAddNew()
    {
        string newName = _newItemTextBox?.Text?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(newName))
        {
            var existingItem = FilteredItems.FirstOrDefault(i => string.Equals(i.Name, newName, StringComparison.OrdinalIgnoreCase));

            if (existingItem != null)
            {
                existingItem.IsSelected = true;
            }
            else
            {
                // Delegasikan penambahan item ke ViewModel/Consumer melalui Command atau Event
                ItemAdded?.Invoke(this, newName);
                if (AddNewCommand != null && AddNewCommand.CanExecute(newName))
                {
                    AddNewCommand.Execute(newName);
                }
            }

            SyncTextFromSelection();
        }

        CancelAddNew();
    }

    private void CancelAddNew()
    {
        if (_newItemTextBox != null)
        {
            _newItemTextBox.Text = string.Empty;
        }
        IsAddingNew = false;
    }

    // --- SELECTION & TEXT SYNC ---

    private void SyncFilteredItems()
    {
        FilteredItems.Clear();
        if (ItemsSource == null) return;

        foreach (var item in ItemsSource)
        {
            if (item is SelectableItem selectable)
            {
                FilteredItems.Add(selectable);
            }
        }
    }

    private void SubscribeToItems(IEnumerable? items)
    {
        if (items == null) return;

        // 🔑 1. Dengarkan jika ada penambahan / penghapusan item di ObservableCollection
        if (items is INotifyCollectionChanged observable)
        {
            observable.CollectionChanged += OnItemsSourceCollectionChanged;
        }

        foreach (var item in items)
        {
            if (item is SelectableItem selectable)
                selectable.PropertyChanged += OnItemPropertyChanged;
        }
    }

    private void UnsubscribeFromItems(IEnumerable? items)
    {
        if (items == null) return;

        if (items is INotifyCollectionChanged observable)
        {
            observable.CollectionChanged -= OnItemsSourceCollectionChanged;
        }

        foreach (var item in items)
        {
            if (item is SelectableItem selectable)
                selectable.PropertyChanged -= OnItemPropertyChanged;
        }
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is SelectableItem selectable)
                {
                    selectable.PropertyChanged += OnItemPropertyChanged;
                    if (!FilteredItems.Contains(selectable))
                    {
                        FilteredItems.Add(selectable);
                    }
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            SyncFilteredItems();
        }

        SyncSelectionFromText();
        SyncTextFromSelection();
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectableItem.IsSelected) && !_isUpdatingSelectionInternally)
        {
            SyncTextFromSelection();
        }
    }

    private void SyncTextFromSelection()
    {
        _isUpdatingTextInternally = true;
        try
        {
            var selectedNames = FilteredItems
                .Where(i => i.IsSelected)
                .Select(i => i.Name.Trim())
                .ToList();

            Text = selectedNames.Count > 0 ? string.Join(", ", selectedNames) : string.Empty;
        }
        finally
        {
            _isUpdatingTextInternally = false;
        }
    }

    private void SyncSelectionFromText()
    {
        _isUpdatingSelectionInternally = true;
        try
        {
            var enteredTags = (Text ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var item in FilteredItems)
            {
                item.IsSelected = enteredTags.Contains(item.Name);
            }
        }
        finally
        {
            _isUpdatingSelectionInternally = false;
        }
    }
}