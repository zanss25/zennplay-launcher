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

        public IEnumerable<long>? SectionIds { get; set; }
        public IEnumerable<long>? SeriesIds { get; set; }
        public IEnumerable<long>? GenreIds { get; set; }
        public IEnumerable<long>? TagIds { get; set; }
        public IEnumerable<long>? DeveloperIds { get; set; }
        public IEnumerable<long>? PublisherIds { get; set; }
        public IEnumerable<long>? PlatformIds { get; set; }
        public IEnumerable<long>? AgeRatingIds { get; set; }

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
