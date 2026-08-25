using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public class AppLinkModel : IEditableListItem
    {
        public Guid Id { get; set; }
        public Guid AppId { get; set; }
        public string? Name { get; set; }
        public string? Desc { get; set; }
        public string Url { get; set; } = string.Empty;   
        public int SortOrder { get; set; }

        // 🔑 Mapping untuk IEditableListItem
        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "(Untitled)" : Name;
        public string DisplayValue => Url;
        public IImage? DisplayIcon => null;
    }

}
