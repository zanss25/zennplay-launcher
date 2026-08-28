using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public partial class ScannedAppModel : ObservableObject
    {
        [ObservableProperty] private bool _isSelected;

        public Bitmap? Icon { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string AppDir { get; set; } = string.Empty;
    }
}
