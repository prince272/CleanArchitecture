namespace CleanArchitecture.Server.Models
{
    public class PageModel<TModel>
    {
        public PageModel(TModel[] items, long totalItems, int page, int pageSize, int pageLimit = 10)
        {
            TotalPages = Math.Max((int)Math.Ceiling(totalItems / (decimal)pageSize), 1);
            Items = items;
            TotalItems = totalItems;
            Page = page;
            PageSize = pageSize;
            PageLimit = pageLimit;
        }

        public TModel[] Items { get; }
        public long TotalItems { get; }
        public int TotalPages { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int PageLimit { get; }
    }
}
