using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public class DeveloperModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Desc { get; set; }
        public string? CreatedAt { get; set; }
    }

    public class AppDeveloperModel
    {
        public int Id { get; set; }
        public int AppId { get; set; }
        public int DeveloperId { get; set; }
    }

}
