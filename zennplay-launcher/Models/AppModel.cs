using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public class AppModel
    {
        public int Id { get; set; }
        public string Guid { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? SortingTitle { get; set; }
        public string? Desc { get; set; }
        public int UserRating { get; set; } = -1;
        public bool IsFavorite { get; set; }
        public bool IsPinned { get; set; }
        public string? Notes { get; set; }
        public string? FirstLaunched { get; set; }
        public string? LastLaunched { get; set; }
        public int LaunchTimes { get; set; } = 0;
        public string? CompletionStatus { get; set; } = "Not Completed";
        public string? ReleaseDate { get; set; }
        public string? AgeRating { get; set; }
        public bool IsInstalled { get; set; } = true;

        // Tri-state override (null = ikuti Section, true = Game, false = App)
        public bool? IsGame { get; set; }
        public bool? IsExcluded { get; set; }
        public string? Version { get; set; }
        public string? AppDir { get; set; }
        public string? ExternalId { get; set; }
        public string? CoverPath { get; set; }
        public string? BackgroundPath { get; set; }
        public string? IconPath { get; set; }
        public string? CreatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Daftar ID relasi yang dipilih dari UI / Form Edit
        public List<long> SeriesIds { get; set; } = new();
        public List<long> GenreIds { get; set; } = new();
        public List<long> TagIds { get; set; } = new();
        public List<long> DeveloperIds { get; set; } = new();
        public List<long> PublisherIds { get; set; } = new();
        public List<long> PlatformIds { get; set; } = new();
    }

}
