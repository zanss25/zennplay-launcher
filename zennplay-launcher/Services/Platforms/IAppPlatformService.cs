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

    // Resolusi metadata: Title, Executable/Target Path akhir (setelah dereference shortcut), dan Base Directory
    (string Title, string ResolvedTargetPath, string Directory) ResolveAppMetadata(string targetPath);

    // Deteksi root game directory untuk keperluan kalkulasi size & scanning
    string DetectRootGameDirectory(string resolvedTargetPath, string defaultDir);

    // Logika menjalankan game / program di OS bersangkutan
    Task LaunchAppAsync(string targetPath, string workingDir, string arguments);
}