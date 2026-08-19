using Avalonia.Media;

namespace zennplay_launcher.Models;

public interface IEditableListItem
{
    string DisplayName { get; }
    string DisplayValue { get; }
    IImage? DisplayIcon { get; }
}