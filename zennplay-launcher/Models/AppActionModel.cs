using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public class AppActionModel : IEditableListItem
    {
        public Guid Id { get; set; }
        public Guid AppId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Desc { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty; // "executable", "url", dll
        public string Target { get; set; } = string.Empty;
        public string? LaunchOptions { get; set; }
        public string? ActionIconPath { get; set; }
        public bool IsGame { get; set; } = true;
        public int SortOrder { get; set; }

        // 🔑 Mapping untuk IEditableListItem
        public string DisplayName => string.IsNullOrWhiteSpace(Title) ? "(Untitled)" : Title;
        public string DisplayValue => Target;
        public IImage? DisplayIcon
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ActionIconPath) && File.Exists(ActionIconPath))
                {
                    try
                    {
                        return new Bitmap(ActionIconPath);
                    }
                    catch
                    {
                        return null; 
                    }
                }

                return null;
            }
        }

    }

}
