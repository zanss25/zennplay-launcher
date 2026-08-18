using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public class AppLinkModel
    {
        public Guid Id { get; set; }
        public Guid AppId { get; set; }
        public string? Name { get; set; }
        public string Url { get; set; } = string.Empty;
    }

}
