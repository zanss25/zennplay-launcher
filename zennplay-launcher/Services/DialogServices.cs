using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Views;

namespace zennplay_launcher.Services
{
    public class DialogServices
    {
        public static async Task<bool> ShowConfirmAsync(Window parent, string title, string message, string action = "Ok", string cancel = "Cancel")
        {
            var popup = new PopupView(title, message, action, cancel);
            return await popup.ShowDialog<bool>(parent);
        }

        //public static async Task ShowAlertAsync(Window parent, string title, string message)
        //{
            
        //}
    }
}
