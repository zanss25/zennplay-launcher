using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Services.Repositories;

namespace zennplay_launcher.ViewModels.Controls
{
    public partial class PropertiesFoldersViewModel : ViewModelBase
    {
        // ============================================================
        // VARIABLES
        // ============================================================

        // Repositories
        private readonly AppRepository _appRepo= new();
        private readonly SectionRepository _sectionRepo = new();

        // Directories List
        // 1. Installation Directory Data
        [ObservableProperty]
        private string _installationFolder = "C:/Games/Dragon Age Origins";
    }
}
