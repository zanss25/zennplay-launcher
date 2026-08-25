using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.ViewModels
{
    public partial class AppLinkPropertiesViewModel : ViewModelBase
    {
        // ============================================================
        // FORM INPUT PROPERTIES
        // ============================================================
        [ObservableProperty] private Guid _id;
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private string _desc = string.Empty;
        [ObservableProperty] private string _url = string.Empty;

        // ============================================================
        // VARIABLES & DIALOG CONTROL
        // ============================================================
        private AppLinkModel? _editingLink;
        public bool IsSaved { get; private set; } = false;
        public event Action? RequestClose;

        // Memuat Data dari Model ke Form
        public void LoadFromModel(AppLinkModel link)
        {
            _editingLink = link; // Simpan referensi objek asli

            Id = link.Id;
            Name = link.Name ?? string.Empty;
            Desc = link.Desc ?? string.Empty;
            Url = link.Url ?? string.Empty;
        }

        // Menerapkan Nilai Form Kembali ke Model
        public void ApplyToModel(AppLinkModel link)
        {
            link.Name = Name;
            link.Desc = Desc;
            link.Url = Url;
        }

        // Command Save saat user klik tombol Save di jendela Edit Action
        [RelayCommand]
        private void Save()
        {
            if (_editingLink != null 
                & !String.IsNullOrEmpty(Name) 
                & !String.IsNullOrEmpty(Url))
            {
                ApplyToModel(_editingLink);

                IsSaved = true;
                RequestClose?.Invoke();
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            IsSaved = false;
            RequestClose?.Invoke();
        }

    }
}
