using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using zennplay_launcher.ViewModels;

namespace zennplay_launcher.Views
{
    public partial class NewAppSelectionWindow : Window
    {
        public NewAppSelectionWindow()
        {
            InitializeComponent();

            DataContext = new NewAppSelectionViewModel();
            Title = "Select Installed Games/Apps";
            TitleText.Text = Title.ToUpper();
        }

    }
}