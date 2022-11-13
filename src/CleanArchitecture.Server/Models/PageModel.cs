namespace CleanArchitecture.Server.Models
{
    public class PageResult
    {
        public PageResult(int totalItems, int pageNumber, int pageSize, int maxPageSize = 12)
        {
            if (pageNumber == 0) pageNumber = 1;
            if (pageSize == 0) pageSize = maxPageSize;

            // Ensure the total pages isn't out of range.
            var totalPages = Math.Max((int)Math.Ceiling(totalItems / (decimal)pageSize), 1);

            // Ensure the current page isn't out of range.
            pageNumber = Math.Min(Math.Max(1, pageNumber), totalPages);

            // Ensure the page size isn't out of range.
            pageSize = Math.Min(Math.Max(1, pageSize), maxPageSize);

            var skippedItems = (pageNumber - 1) * pageSize;

            PageNumber = pageNumber;
            PageSize = pageSize;
            SkipItems = skippedItems;
            TotalPages = totalPages;
            TotalItems = totalItems;
        }

        public int PageNumber { get; }

        public int PageSize { get; }

        public int TotalPages { get; }

        public int TotalItems { get; }

        public int SkipItems { get; }

        public object Items { get; set; }
    }
}
