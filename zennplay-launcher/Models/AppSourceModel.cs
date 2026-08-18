using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public partial class AppSourceModel
    {
        public required string Id { get; set; }
        public required string Name { get; set; }

        public override string ToString() => Name;
    }

    public partial class AppSourceList
    {
        public static List<AppSourceModel> AvailableSources =>
        [
            new AppSourceModel { Id = "empty", Name = "" },
            new AppSourceModel { Id = "steam", Name = "Steam" },
            new AppSourceModel { Id = "standalone", Name = "Standalone" }
        ];
    }

}
