using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public class SectionModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Desc { get; set; }
        public string? Icon { get; set; }
        public string? AppType { get; set; }
        public bool IsGameByDefault { get; set; } = true;
        public bool IsExcludedByDefault { get; set; } = false;
        public string? CreatedAt { get; set; }
    }

    public class AppSectionModel
    {
        public int Id { get; set; }
        public int AppId { get; set; }
        public int SectionId { get; set; }
    }

}
