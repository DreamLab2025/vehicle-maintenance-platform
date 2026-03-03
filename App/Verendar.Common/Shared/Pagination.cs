namespace Verendar.Common.Shared
{
    public class PaginationRequest
    {
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = DefaultPageSize;
        public bool? IsDescending { get; set; }

        public virtual void Normalize()
        {
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = DefaultPageSize;
            if (PageSize > MaxPageSize) PageSize = MaxPageSize;
        }
    }

    public class PagingMetadata(int totalItems, int pageNumber, int pageSize)
    {
        public int PageNumber { get; set; } = pageNumber;
        public int PageSize { get; set; } = pageSize;
        public int TotalItems { get; set; } = totalItems;
        public int TotalPages { get; set; } = (int)Math.Ceiling(totalItems / (double)pageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }
}