using AppBL.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AppBL.Helper
{
    /// <summary>
    /// Extension methods for applying pagination to EF Core queries.
    /// </summary>
    public static class PaginationHelper
    {
        public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
            this IQueryable<T> query,
            PagedRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = request.GetNormalizedPageNumber();
            var pageSize = request.GetNormalizedPageSize();

            var totalCount = await query.CountAsync(cancellationToken);
            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResponse<T>
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNext = pageNumber < totalPages,
                HasPrevious = pageNumber > 1 && totalPages > 0
            };
        }
    }
}
