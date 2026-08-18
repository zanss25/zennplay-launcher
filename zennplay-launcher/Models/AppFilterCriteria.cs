using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Services.Repositories;
using static zennplay_launcher.Services.Repositories.AppRepository;

namespace zennplay_launcher.Models
{
    public class AppFilterCriteria
    {
        public string? SearchQuery { get; set; }
        public bool? OnlyFavorites { get; set; }
        public bool? OnlyInstalled { get; set; }
        public bool? OnlyPinned { get; set; }
        public bool? IsGame { get; set; }

        public IEnumerable<Guid>? SectionIds { get; set; }
        public IEnumerable<Guid>? SeriesIds { get; set; }
        public IEnumerable<Guid>? GenreIds { get; set; }
        public IEnumerable<Guid>? TagIds { get; set; }
        public IEnumerable<Guid>? DeveloperIds { get; set; }
        public IEnumerable<Guid>? PublisherIds { get; set; }
        public IEnumerable<Guid>? PlatformIds { get; set; }
        public IEnumerable<Guid>? AgeRatingIds { get; set; }

        public AppSortBy SortBy { get; set; } = AppSortBy.Title;
        public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
    }

    public enum AppSortBy
    {
        Title,
        ReleaseDate,
        UserRating,
        CreatedAt,
        LastPlayed
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }

}
