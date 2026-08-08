using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using zennplay_launcher.Models;
using zennplay_launcher.Services;
using zennplay_launcher.Services.Repositories;
using zennplay_launcher.Themes;

namespace zennplay_launcher.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var dbInitializer = new DatabaseInitializer();
        dbInitializer.Initialize();

        try
        {
            ThemeManager.SetColorScheme("ObsidianPurple");
        } 
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

        

        //SeedTestAppsData();
        //TestSectionRepositoryFlow();
        //TestGenreRepositoryFlow();
        //TestTagRepositoryFlow();
        //TestLaunchSessionRepositoryFlow();
    }



    #region TESTFLOW

    private void SeedTestAppsData()
    {
        var appRepo = new AppRepository();

        // Cek agar data dummy tidak dimasukkan berulang-ulang setiap kali di-run
        var existingApps = appRepo.GetAllApps();
        if (System.Linq.Enumerable.Any(existingApps))
        {
            Console.WriteLine("Data dummy sudah ada di DB, melewati proses seeding dummy.");
            return;
        }

        Console.WriteLine("Memasukkan data dummy testing ke database...");

        // 🎮 GAME 1: Cyberpunk 2077 (Lengkap dengan Actions, Links, Tags, Genres)
        var cyberpunk = new AppModel
        {
            Title = "Cyberpunk 2077",
            SortingTitle = "Cyberpunk 2077",
            Desc = "An open-world, action-adventure RPG set in the megalopolis of Night City.",
            UserRating = 5,
            IsFavorite = true,
            IsPinned = true,
            Series = "Cyberpunk",
            Notes = "Mainkan dengan RTX On. Mod CET sudah terinstall.",
            Developer = "CD PROJEKT RED",
            Publisher = "CD PROJEKT RED",
            ReleaseDate = "2020-12-10",
            AgeRating = "M (17+)",
            IsInstalled = true,
            IsGame = true, // Force sebagai Game
            Version = "2.12",
            AppDir = @"C:\Games\Cyberpunk 2077",
            SourcePlatform = "GOG",
            CoverPath = "covers/cyberpunk.jpg",
            BackgroundPath = "backgrounds/cyberpunk_hero.jpg",
            IconPath = "icons/cyberpunk.ico"
        };

        var cyberpunkActions = new List<AppActionModel>
            {
                new AppActionModel { Title = "Play Cyberpunk 2077", ActionType = "executable", Target = @"C:\Games\Cyberpunk 2077\bin\x64\Cyberpunk2077.exe", IsGame = true, IsPrimary = true },
                new AppActionModel { Title = "Launch REDmod Tool", ActionType = "executable", Target = @"C:\Games\Cyberpunk 2077\tools\REDmod.exe", IsGame = false, IsPrimary = false }
            };

        var cyberpunkLinks = new List<AppLinkModel>
            {
                new AppLinkModel { Name = "Nexus Mods Page", Url = "https://www.nexusmods.com/cyberpunk2077" },
                new AppLinkModel { Name = "Official Map", Url = "https://piggyback.com/us/guide/cyberpunk-2077/" }
            };

        // ID 1 = Games (Default Section), ID 1 = Action, ID 3 = RPG
        appRepo.AddApp(
            app: cyberpunk,
            sectionIds: new List<int> { 1 },
            genreIds: new List<int> { 1, 3 },
            tagIds: new List<int> { 1, 4, 5 }, // Singleplayer, Open World, Story Rich
            actions: cyberpunkActions,
            links: cyberpunkLinks
        );

        // 🎮 GAME 2: Elden Ring
        var eldenRing = new AppModel
        {
            Title = "ELDEN RING",
            SortingTitle = "Elden Ring",
            Desc = "THE NEW FANTASY ACTION RPG. Rise, Tarnished, and be guided by grace to brandish the power of the Elden Ring.",
            UserRating = 5,
            IsFavorite = true,
            IsPinned = false,
            Series = "Soulsborne",
            Developer = "FromSoftware Inc.",
            Publisher = "Bandai Namco Entertainment",
            ReleaseDate = "2022-02-25",
            IsInstalled = true,
            IsGame = true,
            CoverPath = "covers/elden_ring.jpg",
            BackgroundPath = "backgrounds/elden_ring.jpg"
        };

        appRepo.AddApp(
            app: eldenRing,
            sectionIds: new List<int> { 1 },
            genreIds: new List<int> { 1, 3 }, // Action, RPG
            tagIds: new List<int> { 1, 2, 4 }  // Singleplayer, Multiplayer, Open World
        );

        // 🛠️ APP 1: Mod Organizer 2 (Kategori Tools / Non-Game)
        var mo2 = new AppModel
        {
            Title = "Mod Organizer 2",
            Desc = "Advanced mod manager for Skyrim, Fallout, and Cyberpunk.",
            IsFavorite = false,
            IsPinned = true,
            IsInstalled = true,
            IsGame = false, // Explicit non-game
            AppDir = @"C:\Tools\ModOrganizer2",
            CoverPath = "covers/mo2.jpg"
        };

        var mo2Actions = new List<AppActionModel>
            {
                new AppActionModel { Title = "Open MO2", ActionType = "executable", Target = @"C:\Tools\ModOrganizer2\ModOrganizer.exe", IsGame = false, IsPrimary = true }
            };

        // ID 2 = Tools Section
        appRepo.AddApp(
            app: mo2,
            sectionIds: new List<int> { 2 },
            actions: mo2Actions
        );

        // 🎮 GAME 3: Hollow Knight (Indie Game)
        var hollowKnight = new AppModel
        {
            Title = "Hollow Knight",
            Desc = "Forge your own path in Hollow Knight! An epic action adventure through a vast ruined kingdom of insects and heroes.",
            UserRating = 4,
            IsFavorite = false,
            IsPinned = false,
            Developer = "Team Cherry",
            Publisher = "Team Cherry",
            ReleaseDate = "2017-02-24",
            IsInstalled = true,
            IsGame = true
        };

        appRepo.AddApp(
            app: hollowKnight,
            sectionIds: new List<int> { 1 },
            genreIds: new List<int> { 1, 2, 8 }, // Action, Adventure, Platformer
            tagIds: new List<int> { 1, 8, 9 }   // Singleplayer, Pixel Art / Indie
        );

        Console.WriteLine("Selesai memasukkan 4 data dummy ke SQLite!");
    }

    private void TestSectionRepositoryFlow()
    {
        var sectionRepo = new SectionRepository();
        var appRepo = new AppRepository();

        Debug.WriteLine("\n==========================================");
        Debug.WriteLine("   STARTING SECTION REPOSITORY TEST FLOW   ");
        Debug.WriteLine("==========================================\n");

        // --- A. SEEDING 10 SECTION DUMMY (Test 1 - Test 10) ---
        var existingSections = sectionRepo.GetAllSections().ToList();

        // Cek agar tidak ter-insert berulang kali jika sudah ada lebih dari 3 section bawaan
        if (existingSections.Count < 10)
        {
            Debug.WriteLine("[1] Memasukkan Section Dummy Test 1 s.d. Test 10...");

            for (int i = 1; i <= 10; i++)
            {
                var dummySection = new SectionModel
                {
                    Name = $"Test {i}",
                    Desc = $"Deskripsi dummy untuk testing Section ke-{i}",
                    Icon = "folder",
                    IsGameByDefault = true,
                    IsExcludedByDefault = false
                };

                try
                {
                    int newId = sectionRepo.AddSection(dummySection);
                    Debug.WriteLine($"  -> Berhasil menambah Section: '{dummySection.Name}' (ID: {newId})");
                }
                catch (Exception)
                {
                    // Menangani nama unik jika 'Test X' sudah ada di DB
                    Debug.WriteLine($"  -> Section '{dummySection.Name}' sudah ada, melewatinya.");
                }
            }
        }

        // Fetch ulang daftar section terbaru
        var allSections = sectionRepo.GetAllSections().ToList();
        Debug.WriteLine($"\n[INFO] Total Section saat ini: {allSections.Count}");
        foreach (var sec in allSections)
        {
            Debug.WriteLine($"  - ID: {sec.Id} | Name: {sec.Name}");
        }

        // --- B. TESTING PEMINDAHAN GAME KE SECTION TEST 1 ---
        var allApps = appRepo.GetAllApps().ToList();
        if (allApps.Any())
        {
            var targetApp = allApps.First();
            var targetSection = allSections.FirstOrDefault(s => s.Name == "Test 1");

            if (targetSection != null)
            {
                Debug.WriteLine($"\n[2] Memindahkan Game '{targetApp.Title}' (ID: {targetApp.Id}) ke Section '{targetSection.Name}' (ID: {targetSection.Id})...");
                sectionRepo.MoveAppToSection(targetApp.Id, targetSection.Id);
                Debug.WriteLine("  -> Berhasil dipindahkan!");
            }
        }

        // --- C. TESTING HAPUS SECTION DENGAN OPSI PENITIPAN (MoveToUnassigned) ---
        var test1Section = allSections.FirstOrDefault(s => s.Name == "Test 1");
        if (test1Section != null)
        {
            Debug.WriteLine($"\n[3] Menghapus Section '{test1Section.Name}' (ID: {test1Section.Id}) dengan Mode Penitipan (MoveToUnassigned)...");

            sectionRepo.DeleteSection(test1Section.Id, SectionRepository.SectionDeleteMode.MoveToUnassigned);
            Debug.WriteLine("  -> Section berhasil dihapus! Game di dalamnya otomatis dititipkan ke 'Storage'.");

            // Cek Game di Tempat Penitipan (Sectionless)
            var unassignedApps = sectionRepo.GetUnassignedApps().ToList();
            Debug.WriteLine($"\n[INFO] Jumlah Game yang ada di tempat PENITIPAN (Sectionless): {unassignedApps.Count}");
            foreach (var unassigned in unassignedApps)
            {
                Debug.WriteLine($"  - Game Dititipkan: '{unassigned.Title}' (ID: {unassigned.Id})");
            }

            // Coba kembalikan game dari penitipan ke Section 'Test 2'
            var test2Section = allSections.FirstOrDefault(s => s.Name == "Test 2");
            if (unassignedApps.Any() && test2Section != null)
            {
                var appToRestore = unassignedApps.First();
                Debug.WriteLine($"\n[4] Mengambil Game '{appToRestore.Title}' dari Penitipan dan memasukkannya ke '{test2Section.Name}'...");
                sectionRepo.AssignUnassignedAppToSection(appToRestore.Id, test2Section.Id);
                Debug.WriteLine("  -> Game berhasil keluar dari penitipan!");
            }
        }

        // --- D. TESTING PROTEKSI MINIMUM 1 SECTION ---
        Debug.WriteLine("\n[5] Menguji Proteksi 'Minimal 1 Section'...");
        var currentSections = sectionRepo.GetAllSections().ToList();

        // Skenario: Kita mencoba menghapus semua section sampai sisa 1
        foreach (var sec in currentSections)
        {
            try
            {
                sectionRepo.DeleteSection(sec.Id, SectionRepository.SectionDeleteMode.MoveToUnassigned);
                Debug.WriteLine($"  - Menghapus Section ID {sec.Id} ({sec.Name})... Sukses!");
            }
            catch (InvalidOperationException ex)
            {
                // Proteksi berhasil bekerja jika masuk ke Catch block ini!
                Debug.WriteLine($"  -> 🛡️ PROTEKSI BERHASIL TERPICU: {ex.Message}");
            }
        }

        Debug.WriteLine("\n==========================================");
        Debug.WriteLine("    SECTION REPOSITORY TEST FLOW END      ");
        Debug.WriteLine("==========================================\n");
    }

    private void TestGenreRepositoryFlow()
    {
        var genreRepo = new GenreRepository();
        var appRepo = new AppRepository();

        Debug.WriteLine("\n==========================================");
        Debug.WriteLine("   STARTING GENRE REPOSITORY TEST FLOW    ");
        Debug.WriteLine("==========================================\n");

        // A. Add Genre Baru
        Debug.WriteLine("[1] Menambahkan Genre Dummy 'Cyberpunk / Sci-Fi' & 'Metroidvania'...");
        int genreId1 = genreRepo.AddGenre(new GenreModel { Name = "Cyberpunk / Sci-Fi", Desc = "Futuristic setting with high tech and low life." });
        int genreId2 = genreRepo.AddGenre(new GenreModel { Name = "Metroidvania", Desc = "Action-adventure games focused on guided non-linear exploration." });
        Debug.WriteLine($"  -> Genre berhasil dibuat! (ID: {genreId1} & ID: {genreId2})");

        // B. Fetch All & Filter
        var allGenres = genreRepo.GetAllGenres().ToList();
        Debug.WriteLine($"\n[2] Total Genre di DB: {allGenres.Count}");
        foreach (var g in allGenres)
        {
            Debug.WriteLine($"  - ID: {g.Id} | Name: {g.Name}");
        }

        // C. Assign Genre to App
        var apps = appRepo.GetAllApps().ToList();
        if (apps.Any())
        {
            var targetApp = apps.First();
            Debug.WriteLine($"\n[3] Menghubungkan Genre 'Metroidvania' (ID: {genreId2}) ke Game '{targetApp.Title}' (ID: {targetApp.Id})...");
            genreRepo.AssignGenreToApp(targetApp.Id, genreId2);

            // Verify App Genres
            var appGenres = genreRepo.GetGenresByAppId(targetApp.Id).ToList();
            Debug.WriteLine($"  -> Genre milik game '{targetApp.Title}': {string.Join(", ", appGenres.Select(g => g.Name))}");
        }

        // D. Update Genre
        var genreToUpdate = genreRepo.GetGenreById(genreId1);
        if (genreToUpdate != null)
        {
            Debug.WriteLine($"\n[4] Mengubah Nama Genre ID {genreId1}...");
            genreToUpdate.Name = "Sci-Fi / Cyberpunk Updated";
            genreRepo.UpdateGenre(genreToUpdate);
            Debug.WriteLine("  -> Berhasil diperbarui!");
        }

        Debug.WriteLine("\n==========================================");
        Debug.WriteLine("     GENRE REPOSITORY TEST FLOW END       ");
        Debug.WriteLine("==========================================\n");
    }

    private void TestTagRepositoryFlow()
    {
        var tagRepo = new TagRepository();
        var appRepo = new AppRepository();

        Debug.WriteLine("\n==========================================");
        Debug.WriteLine("     STARTING TAG REPOSITORY TEST FLOW    ");
        Debug.WriteLine("==========================================\n");

        // A. Add Tag Baru
        Debug.WriteLine("[1] Menambahkan Tag Dummy 'Must Play' & 'Heavy Modded'...");
        int tagId1 = tagRepo.AddTag(new TagModel { Name = "Must Play", Desc = "Games that are absolute masterpieces." });
        int tagId2 = tagRepo.AddTag(new TagModel { Name = "Heavy Modded", Desc = "Games currently installed with lots of mods." });
        Debug.WriteLine($"  -> Tag berhasil dibuat! (ID: {tagId1} & ID: {tagId2})");

        // B. Fetch & Assign
        var apps = appRepo.GetAllApps().ToList();
        if (apps.Any())
        {
            var targetApp = apps.First();
            Debug.WriteLine($"\n[2] Menghubungkan Tag 'Must Play' (ID: {tagId1}) ke Game '{targetApp.Title}' (ID: {targetApp.Id})...");
            tagRepo.AssignTagToApp(targetApp.Id, tagId1);

            // Verify App Tags
            var appTags = tagRepo.GetTagsByAppId(targetApp.Id).ToList();
            Debug.WriteLine($"  -> Tag milik game '{targetApp.Title}': {string.Join(", ", appTags.Select(t => t.Name))}");

            // C. Remove Tag
            Debug.WriteLine($"\n[3] Melepas Tag 'Must Play' dari Game '{targetApp.Title}'...");
            tagRepo.RemoveTagFromApp(targetApp.Id, tagId1);
            var remainingTags = tagRepo.GetTagsByAppId(targetApp.Id).ToList();
            Debug.WriteLine($"  -> Sisa Tag milik game '{targetApp.Title}': {remainingTags.Count} tag");
        }

        Debug.WriteLine("\n==========================================");
        Debug.WriteLine("      TAG REPOSITORY TEST FLOW END        ");
        Debug.WriteLine("==========================================\n");
    }

    private void TestLaunchSessionRepositoryFlow()
    {
        var sessionRepo = new LaunchSessionRepository();
        var appRepo = new AppRepository();

        Debug.WriteLine("\n==========================================");
        Debug.WriteLine(" STARTING LAUNCH SESSION REPOSITORY FLOW  ");
        Debug.WriteLine("==========================================\n");

        var apps = appRepo.GetAllApps().ToList();
        if (apps.Any())
        {
            var targetApp = apps.First();

            // A. Simulasi Tombol PLAY Diklik (Start Session)
            Debug.WriteLine($"[1] Simulasi Player Mengklik PLAY pada Game '{targetApp.Title}' (ID: {targetApp.Id})...");
            int currentSessionId = sessionRepo.StartSession(targetApp.Id);
            Debug.WriteLine($"  -> Sesi Bermain Dimulai! Session ID: {currentSessionId}");

            // Simulasi Jeda Waktu Bermain Game (2 Detik)
            System.Threading.Thread.Sleep(2000);

            // B. Simulasi Game Ditutup (End Session)
            Debug.WriteLine($"\n[2] Simulasi Game '{targetApp.Title}' Ditutup Oleh Player...");
            sessionRepo.EndSession(currentSessionId, targetApp.Id);
            Debug.WriteLine("  -> Sesi Bermain Berhasil Ditutup & Jam Main Tercatat Otomatis!");

            // C. Verifikasi Riwayat Sesi & Total Jam Main Game Ini
            var appSessions = sessionRepo.GetSessionsByAppId(targetApp.Id).ToList();
            int totalSeconds = sessionRepo.GetTotalPlaytimeSecondsByAppId(targetApp.Id);

            Debug.WriteLine($"\n[INFO] Total Sesi Bermain Game '{targetApp.Title}': {appSessions.Count} kali");
            Debug.WriteLine($"[INFO] Total Playtime Game Ini: {totalSeconds} Detik");
            foreach (var s in appSessions)
            {
                Debug.WriteLine($"  - [Session #{s.Id}] Start: {s.SessionStart} | End: {s.SessionEnd} | Durasi: {s.DurationSeconds}s");
            }

            // D. Verifikasi Activity Log Global Recent Sessions
            var recentGlobal = sessionRepo.GetRecentGlobalSessions(5).ToList();
            Debug.WriteLine($"\n[INFO] Activity Log Global (Terakhir Dimainkan): {recentGlobal.Count} sesi tercatat");
        }

        Debug.WriteLine("\n==========================================");
        Debug.WriteLine("  LAUNCH SESSION REPOSITORY FLOW END      ");
        Debug.WriteLine("==========================================\n");
    }

    #endregion



}