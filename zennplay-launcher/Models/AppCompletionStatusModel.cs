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

    
}
