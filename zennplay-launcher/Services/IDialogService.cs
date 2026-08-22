using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Services
{
    public interface IDialogService
    {
        Task<Guid?> ShowNewAppSelectionAsync();
        Task<bool> ShowAppPropertiesAsync(Guid appId, bool isNew = false, Guid? initialSectionId = null);
    }
}
