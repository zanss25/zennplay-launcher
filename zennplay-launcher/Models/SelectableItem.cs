using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace zennplay_launcher.Models;

public class SelectableItem : INotifyPropertyChanged
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public SelectableItem(string name, int id = 0, string? description = null, bool isSelected = false)
    {
        Name = name;
        Id = id;
        Description = description;
        IsSelected = isSelected;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}