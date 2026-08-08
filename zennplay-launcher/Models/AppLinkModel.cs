using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public class AppLinkModel
    {
        public int Id { get; set; }
        public int AppId { get; set; }
        public string? Name { get; set; }
        public string Url { get; set; } = string.Empty;
    }

}
