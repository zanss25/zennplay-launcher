using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace zennplay_launcher.Services;

public interface IAppPlatformService
{
    // Filter file picker sesuai format OS
    IReadOnlyList<FilePickerFileType> GetExecutableFileFilters();

    // Validasi apakah target bisa dieksekusi di OS ini
    bool IsExecutable(string targetPath);

    // Resolusi nama dan direktori dari target path (misal extract dari .lnk atau .desktop)
    (string Title, string Directory) ResolveAppMetadata(string targetPath);

    // Logika menjalankan game / program di OS bersangkutan
    Task LaunchAppAsync(string targetPath, string workingDir, string arguments);
}