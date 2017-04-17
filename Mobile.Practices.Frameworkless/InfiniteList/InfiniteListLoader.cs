using MetroLog;

using Mobile.Practices.Frameworkless.Core;

namespace Mobile.Practices.Frameworkless.InfiniteList
{
    /// <summary>
    /// Class InfiniteListLoader.
    /// </summary>
    public class InfiniteListLoader : IInfiniteListLoader
    {
        private static readonly ILogger Log = LoggerFactory.GetLogger(nameof(InfiniteListLoader));
        
        private readonly IPagedListProvider _pagedListProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfiniteListLoader"/> class.
        /// </summary>
        /// <param name="pagedListProvider">The paged list provider.</param>
        public InfiniteListLoader(IPagedListProvider pagedListProvider)
        {
            _pagedListProvider = pagedListProvider;
        }


        /// <summary>
        /// Called when [scroll].
        /// </summary>
        public void OnScroll(int maxVisibleIndex)
        {
            if (_pagedListProvider.LoadedCount >= _pagedListProvider.TotalCount)
            {
                // All messages are already loaded nothing to paginate
                return;
            }

            int itemsCount = _pagedListProvider.LoadedCount;
            if (maxVisibleIndex > (itemsCount - _pagedListProvider.PageSize))
            {
                Log.Info(
                    $"{GetType().Name} => Scrolled: loading more (max index of visible item {maxVisibleIndex} / loaded items count {itemsCount}");

                int nextPageToLoad = (itemsCount / _pagedListProvider.PageSize) + 1;
                _pagedListProvider.LoadPage(nextPageToLoad);
            }
        }
    }
}