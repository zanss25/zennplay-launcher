using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public class SectionOverridesModel
    {
        public PostLaunchActions SectionDefaultLaunchAction { get; set; } = PostLaunchActions.MinimizeLauncher;
        public string SectionDefaultSortOption { get; set; } = "title";
        public SectionViewModes SectionDefaultViewMode { get; set; } = SectionViewModes.GridView;
    }
}
