using System.ComponentModel.DataAnnotations;

namespace AppBL.DTOs
{
    /// <summary>
    /// Standard pagination request parameters.
    /// </summary>
    public class PagedRequestDto
    {
        private const int MaxPageSize = 100;

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, MaxPageSize)]
        public int PageSize { get; set; } = 10;

        /// <summary>Optional search term applied per endpoint.</summary>
        public string? Search { get; set; }

        public int GetNormalizedPageNumber() => PageNumber < 1 ? 1 : PageNumber;

        public int GetNormalizedPageSize() => PageSize < 1 ? 10 : (PageSize > MaxPageSize ? MaxPageSize : PageSize);
    }

    /// <summary>
    /// Generic paginated response wrapper.
    /// </summary>
    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }
}
