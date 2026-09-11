using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Models
{
    public partial class UISortCriteriaModel
    {
        public Enum CriteriaId { get; set; } = default!;
        public string? DisplayName { get; set; }
    }

    public partial class UISortByList
    {
        public static List<UISortCriteriaModel> UISortBy =>
        [
            new UISortCriteriaModel { CriteriaId = AppSortBy.Title, DisplayName = "Title" },
            new UISortCriteriaModel { CriteriaId = AppSortBy.ReleaseDate, DisplayName = "Release Date" },
            new UISortCriteriaModel { CriteriaId = AppSortBy.UserRating, DisplayName = "User Rating" },
            new UISortCriteriaModel { CriteriaId = AppSortBy.CreatedAt, DisplayName = "Date Added" },
            new UISortCriteriaModel { CriteriaId = AppSortBy.LastLaunched, DisplayName = "Last Launched" }
        ];

    }

    public partial class UISortDirectionList
    {
        public static List<UISortCriteriaModel> UISortDirection =>
        [
            new UISortCriteriaModel { CriteriaId = SortDirection.Ascending, DisplayName = "ASC" },
            new UISortCriteriaModel { CriteriaId = SortDirection.Descending, DisplayName = "DSC" }
        ];
    }
}
