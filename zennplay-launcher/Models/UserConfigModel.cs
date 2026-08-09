using System.Text.Json.Serialization;

namespace zennplay_launcher.Models
{
    // ============================================================
    // MAIN CONFIG MODEL
    // ============================================================
    public partial class UserConfigModel
    {
        public GeneralSettings General { get; set; } = new GeneralSettings();
        public AppearanceSettings Appearance { get; set; } = new AppearanceSettings();
        public DirectoriesSettings Directories { get; set; } = new DirectoriesSettings();
        public BackupSettings Backup { get; set; } = new BackupSettings();
        public AdvancedSettings Advanced { get; set; } = new AdvancedSettings();
    }

    // ============================================================
    // ENUMS
    // ============================================================
    public enum Modifiers
    {
        Shift,
        Ctrl,
        Alt
    }
    public enum PostLaunchActions
    {
        MinimizeLauncher,
        CloseLauncher,
        DoNothing
    }
    public enum SectionViewModes
    {
        GridView,
        ListView
    }

    // ============================================================
    // SETTINGS SECTIONS
    // ============================================================
    public partial class GeneralSettings
    {
        public int DefaultStartingSectionId { get; set; } = 0;
        public bool CloseToTray { get; set; } = true;
        public bool StartAtLogin { get; set; } = true;
        public PostLaunchActions PostLaunchAction { get; set; } = PostLaunchActions.MinimizeLauncher;
        public Modifiers BypassLaunchActionModifier { get; set; } = Modifiers.Shift;
        public string DefaultSortOption { get; set; } = "title";
        public SectionViewModes DefaultViewMode { get; set; } = SectionViewModes.GridView;
    }
    public partial class AppearanceSettings
    {
        public string ActiveTheme { get; set; } = "ObsidianPurple";
        public string ActiveFontCommon { get; set; } = "Segoe UI";
        public string ActiveFontPrimary { get; set; } = "Segoe UI";
        public bool EnableWindowBlur { get; set; } = true;
        public int BlurIntensity { get; set; } = 20;
        public double DimOpacity { get; set; } = 30;
        public bool CoverRoundedCorners { get; set; } = false;
        public int CoverCornerRadius { get; set; } = 8; 
    }
    public partial class DirectoriesSettings
    {
        public string DefaultAppsDir { get; set; } = string.Empty;
        public string DefaultCoversSourceDir { get; set; } = string.Empty;
        public string DefaultIconsSourceDir { get; set; } = string.Empty;
    }
    public partial class BackupSettings
    {
        public bool EnableAutoBackup { get; set; } = true;
        public double AutoBackupFrequency { get; set; } = 30; // dalam hari
        public string BackupCustomDestination { get; set; } = string.Empty;
    }
    public partial class AdvancedSettings
    {
        public bool EnableLogging { get; set; } = false;
    }
}