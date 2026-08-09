using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public partial class AppOverridesModel
    {
        public PostLaunchActions AppDefaultLaunchAction { get; set; } = PostLaunchActions.MinimizeLauncher;
    }
}
