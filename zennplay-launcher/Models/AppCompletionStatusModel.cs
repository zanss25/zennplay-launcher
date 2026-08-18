using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public partial class AppCompletionStatusModel
    {
        public required string Id { get; set; }
        public required string Name { get; set; }

        public override string ToString() => Name;
    }

    public partial class AppCompletionStatusList
    {
        public static List<AppCompletionStatusModel> AvailableCompletionStatuses =>
        [
            new AppCompletionStatusModel { Id = "empty", Name = "" },
            new AppCompletionStatusModel { Id = "not_played", Name = "Not Played" },
            new AppCompletionStatusModel { Id = "plan_to_play", Name = "Plan to Play" },
            new AppCompletionStatusModel { Id = "playing", Name = "Playing" },
            new AppCompletionStatusModel { Id = "played", Name = "Played" },
            new AppCompletionStatusModel { Id = "beaten", Name = "Beaten" },
            new AppCompletionStatusModel { Id = "on_hold", Name = "On Hold" },
            new AppCompletionStatusModel { Id = "abandoned", Name = "Plan to Play" },
        ];
    }
}
