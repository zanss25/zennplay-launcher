using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public partial class SidebarTabModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }

    }

    public partial class SidebarTabsList
    {
        public static List<SidebarTabModel> DefaultTabs => new()
        {
            new SidebarTabModel { Id = "library", Name = "Library", Icon = "LibIcon" },
            new SidebarTabModel { Id = "favorites", Name = "Favorites", Icon = "StarIcon" },
            new SidebarTabModel { Id = "boards", Name = "Boards", Icon = "BoardIcon" },
            new SidebarTabModel { Id = "relics", Name = "Relics", Icon = "TrophyIcon" }
        };
    }

}
