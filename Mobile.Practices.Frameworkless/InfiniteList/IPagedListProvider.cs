namespace Mobile.Practices.Frameworkless.InfiniteList
{
    /// <summary>
    /// The PagedListProvider interface.
    /// </summary>
    public interface IPagedListProvider
    {
        /// <summary>
        /// Gets the total count.
        /// </summary>
        int TotalCount { get; }

        /// <summary>
        /// Gets the loaded count.
        /// </summary>
        int LoadedCount { get; }

        /// <summary>
        /// Gets the page size.
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// Loads the page n°pagenumber.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        void LoadPage(int pageNumber);
    }
}