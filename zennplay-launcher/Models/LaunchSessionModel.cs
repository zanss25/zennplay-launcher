using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public class LaunchSessionModel
    {
        public Guid Id { get; set; }
        public Guid AppId { get; set; }
        public string SessionStart { get; set; } = string.Empty;
        public string? SessionEnd { get; set; }
        public long? DurationSeconds { get; set; }
    }
}
