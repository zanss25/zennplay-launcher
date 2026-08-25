using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services
{
    public interface IDialogService
    {
        Task<Guid?> ShowNewAppSelectionAsync();
        Task<bool> ShowAppPropertiesAsync(Guid appId, bool isNew = false, Guid? initialSectionId = null);
        Task<bool> ShowAppActionPropertiesAsync(AppActionModel action);
        Task<bool> ShowAppLinkPropertiesAsync(AppLinkModel link);
        Task<bool> ShowConfirmAsync(string title, string message, string action = "Ok", bool showCancel = true, 
                                    string cancel = "Cancel");
        Task<string> ShowInputAsync(string title, string message, string text, string action = "Ok", 
                                    string cancel = "Cancel");
    }
}
