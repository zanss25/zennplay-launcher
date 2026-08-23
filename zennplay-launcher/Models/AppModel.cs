using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public class AppModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? SortingTitle { get; set; }
        public string? Desc { get; set; }
        public int UserRating { get; set; } = -1;
        public bool IsFavorite { get; set; }
        public bool IsPinned { get; set; } = false;
        public string? Notes { get; set; }
        public DateTime? FirstLaunched { get; set; }
        public DateTime? LastLaunched { get; set; }
        public int LaunchTimes { get; set; } = 0;
        public string? CompletionStatus { get; set; } = "Not Completed";
        public string? ReleaseDate { get; set; }
        public string? AgeRating { get; set; }
        public bool IsInstalled { get; set; } = true;

        // Tri-state override (null = ikuti Section, true = Game, false = App)
        public bool IsGame { get; set; } = true;
        public bool IsExcluded { get; set; } = false;
        public string? Version { get; set; }
        public string? AppDir { get; set; }
        public long InstallSize { get; set; }
        public string? Source { get; set; }
        public string? ExternalId { get; set; }
        public string? CoverPath { get; set; }
        public string? BackgroundPath { get; set; }
        public string? IconPath { get; set; }
        public string? CreatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Daftar ID relasi yang dipilih dari UI / Form Edit
        public List<Guid> SectionIds { get; set; } = new();
        public List<Guid> SeriesIds { get; set; } = new();
        public List<Guid> GenreIds { get; set; } = new();
        public List<Guid> TagIds { get; set; } = new();
        public List<Guid> DeveloperIds { get; set; } = new();
        public List<Guid> PublisherIds { get; set; } = new();
        public List<Guid> PlatformIds { get; set; } = new();
        public List<Guid> AgeRatingIds { get; set; } = new();
    }

}
