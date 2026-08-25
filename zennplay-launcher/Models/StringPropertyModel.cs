using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public partial class StringPropertyModel
    {
        public required string Id { get; set; }
        public required string Name { get; set; }

        public override string ToString() => Name;
    }

    public partial class AppSources
    {
        public static List<StringPropertyModel> AvailableSources =>
        [
            new StringPropertyModel { Id = "empty", Name = "" },
            new StringPropertyModel { Id = "steam", Name = "Steam" },
            new StringPropertyModel { Id = "standalone", Name = "Standalone" }
        ];
    }

    public partial class AppCompletionStatuses
    {
        public static List<StringPropertyModel> AvailableCompletionStatuses =>
        [
            new StringPropertyModel { Id = "empty", Name = "" },
            new StringPropertyModel { Id = "not_played", Name = "Not Played" },
            new StringPropertyModel { Id = "plan_to_play", Name = "Plan to Play" },
            new StringPropertyModel { Id = "playing", Name = "Playing" },
            new StringPropertyModel { Id = "played", Name = "Played" },
            new StringPropertyModel { Id = "beaten", Name = "Beaten" },
            new StringPropertyModel { Id = "on_hold", Name = "On Hold" },
            new StringPropertyModel { Id = "abandoned", Name = "Abandoned" },
        ];
    }

    public partial class AppActionTypes
    {
        public static List<StringPropertyModel> AvailableActionTypes =>
        [
            new StringPropertyModel { Id = "executable", Name = "Executable" },
            new StringPropertyModel { Id = "textconfig", Name = "Text/Config File" },
            new StringPropertyModel { Id = "document", Name = "Document" },
            new StringPropertyModel { Id = "other", Name = "Other" }
        ];
    }

}
