using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public class UserOverridesModel
    {
        // Key: Section ID (ex. "1")
        public Dictionary<string, SectionOverridesModel> SectionOverrides { get; set; } = [];

        // Key: App ID (ex. "10")
        public Dictionary<string, AppOverridesModel> AppOverrides { get; set; } = [];
    }
}

