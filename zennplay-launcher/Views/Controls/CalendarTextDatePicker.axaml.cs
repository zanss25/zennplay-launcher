using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Calendar = Avalonia.Controls.Calendar;
namespace zennplay_launcher.Views.Controls;

public class CalendarTextDatePicker : TemplatedControl
{
    private TextBox? _textBox;
    private Calendar? _calendar;
    private bool _isInternalUpdating;

    protected override Type StyleKeyOverride => typeof(CalendarTextDatePicker);

    // 1. Text Property
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<CalendarTextDatePicker, string?>(
            nameof(Text),
            defaultBindingMode: BindingMode.TwoWay);

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    // 2. SelectedDate Property (DateTime?)
    public static readonly StyledProperty<DateTime?> SelectedDateProperty =
        AvaloniaProperty.Register<CalendarTextDatePicker, DateTime?>(
            nameof(SelectedDate),
            defaultBindingMode: BindingMode.TwoWay);

    public DateTime? SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    // 3. DateFormat (Default: yyyy-MM-dd)
    public static readonly StyledProperty<string> DateFormatProperty =
        AvaloniaProperty.Register<CalendarTextDatePicker, string>(nameof(DateFormat), "yyyy-MM-dd");

    public string DateFormat
    {
        get => GetValue(DateFormatProperty);
        set => SetValue(DateFormatProperty, value);
    }

    // 4. PlaceholderText
    public static readonly StyledProperty<string> PlaceholderTextProperty =
        AvaloniaProperty.Register<CalendarTextDatePicker, string>(nameof(PlaceholderText), "YYYY-MM-DD");

    public string PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    // 5. Dropdown Popup State
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<CalendarTextDatePicker, bool>(
            nameof(IsDropDownOpen),
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_textBox != null)
            _textBox.LostFocus -= OnTextBoxLostFocus;

        if (_calendar != null)
            _calendar.SelectedDatesChanged -= OnCalendarSelectedDatesChanged;

        _textBox = e.NameScope.Find<TextBox>("PART_TextBox");
        _calendar = e.NameScope.Find<Calendar>("PART_Calendar");

        if (_textBox != null)
            _textBox.LostFocus += OnTextBoxLostFocus;

        if (_calendar != null)
            _calendar.SelectedDatesChanged += OnCalendarSelectedDatesChanged;

        SyncTextFromDate();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedDateProperty && !_isInternalUpdating)
        {
            SyncTextFromDate();
        }
        else if (change.Property == TextProperty && !_isInternalUpdating)
        {
            SyncDateFromText();
        }
    }

    private void OnCalendarSelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isInternalUpdating) return;

        if (_calendar?.SelectedDate != null)
        {
            SelectedDate = _calendar.SelectedDate;
            IsDropDownOpen = false; // Otomatis tutup popup setelah tanggal dipilih
        }
    }

    private void OnTextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        SyncDateFromText();
    }

    private void SyncTextFromDate()
    {
        _isInternalUpdating = true;
        try
        {
            Text = SelectedDate?.ToString(DateFormat, CultureInfo.InvariantCulture) ?? string.Empty;
        }
        finally
        {
            _isInternalUpdating = false;
        }
    }

    private void SyncDateFromText()
    {
        _isInternalUpdating = true;
        try
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                SelectedDate = null;
                return;
            }

            if (DateTime.TryParse(Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate) ||
                DateTime.TryParse(Text, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsedDate))
            {
                SelectedDate = parsedDate;
            }
        }
        finally
        {
            _isInternalUpdating = false;
        }
    }
}