using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public class AppActionModel
    {
        public Guid Id { get; set; }
        public Guid AppId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Desc { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty; // "executable", "url", dll
        public string Target { get; set; } = string.Empty;
        public string? LaunchOptions { get; set; }
        public string? ActionIconPath { get; set; }
        public bool IsGame { get; set; }
        public bool IsPrimary { get; set; }
    }

}
