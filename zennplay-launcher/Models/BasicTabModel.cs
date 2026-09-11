using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public partial class BasicTabModel
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? Icon { get; set; }
    }

    public partial class PropertiesTabsList
    {
        public static List<BasicTabModel> PropertiesTabs =>
        [
            new BasicTabModel { Id = "metadata", Name = "Metadata" },
            new BasicTabModel { Id = "folders", Name = "Folders & Actions" },
            new BasicTabModel { Id = "media", Name = "Customization" }
        ];

    }

    public partial class AppViewModesList
    {
        public static List<BasicTabModel> AppViewModes =>
        [
            new BasicTabModel { Id = "grid", Name = "GR" },
            new BasicTabModel { Id = "list", Name = "LS" },
            new BasicTabModel { Id = "panel", Name = "PN" }
        ];
    }

}
