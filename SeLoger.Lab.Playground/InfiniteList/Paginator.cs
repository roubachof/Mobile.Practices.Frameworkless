using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

using MetroLog;

using SeLoger.Contracts;
using SeLoger.Lab.Playground.Core;
using SeLoger.Lab.Playground.Core.Nito;
using SeLoger.Lab.Playground.Services;

namespace SeLoger.Lab.Playground.InfiniteList
{
    public class Paginator<TResult> : IPagedListProvider, IInfiniteListLoader
    {
        private static readonly ILogger Log = LoggerFactory.GetLogger(nameof(Paginator<TResult>));

        private readonly object _syncRoot = new object();

        private readonly int _maxItemCount;

        private readonly Func<int, int, Task<PageResult<TResult>>> _pageSourceLoader;

        private readonly Action _onTaskCompleted;
        
        private bool _refreshRequested;

        public Paginator(int pageSize, int maxItemCount, Func<int, int, Task<PageResult<TResult>>> pageSourceLoader, Action onTaskCompleted)
        {
            Contract.Requires(() => pageSize > 10);
            Contract.Requires(() => maxItemCount > 10);

            Log.Info($"Building paginator with pageSize: {pageSize}, maxItemCount: {maxItemCount}");
            PageSize = pageSize;
            _maxItemCount = maxItemCount;
            _pageSourceLoader = pageSourceLoader;
            _onTaskCompleted = onTaskCompleted;

            TotalCount = _maxItemCount;
            Reset();
        }

        public int PageLoadedCount { get; private set; }         
        
        public int LoadedCount => Items.Count;

        public bool IsFull => LoadedCount >= TotalCount;

        public int PageSize { get; }
        
        public int TotalCount { get; private set; }

        public bool HasStarted => NotifyTask != null;

        public bool HasRefreshed
        {
            get { lock (_syncRoot) return _refreshRequested; }
        }

        public NotifyTask<PageResult<TResult>> NotifyTask { get; private set; }

        public ImmutableList<TResult> Items { get; private set; }

        public void Reset()
        {
            Log.Info("Resetting paginator");
            PageLoadedCount = 0;
            Items = ImmutableList<TResult>.Empty;
        }

        public void OnScroll(int maxVisibleIndex)
        {
            if (maxVisibleIndex < 0)
            {
                return;
            }

            if (IsFull)
            {
                // All messages are already loaded nothing to paginate
                return;
            }

            if (HasStarted && NotifyTask.IsNotCompleted)
            {
                // Currently loading page
                return;
            }

            int itemsCount = LoadedCount;
            if (maxVisibleIndex > (itemsCount - PageSize * 1/4))
            {
                Log.Info(
                    $"Scrolled: loading more (max index of visible item {maxVisibleIndex} / loaded items count {itemsCount})");

                LoadPage(PageLoadedCount + 1);
            }
        }

        public void LoadPage(int pageNumber)
        {
            Contract.Requires(() => pageNumber > 0);

            Log.Info($"Loading page n°{pageNumber}, {PageLoadedCount} pages loaded so far");
            lock(_syncRoot)
            {
                if (pageNumber > PageLoadedCount && IsFull)
                {
                    Log.Info($"Cannot load page {pageNumber} max item count has already been reached ({_maxItemCount})");
                    return;
                }

                if (pageNumber == 1 && PageLoadedCount > 0)
                {
                    Log.Info("Refresh dectected");
                    _refreshRequested = true;
                }
                else
                {
                    _refreshRequested = false;
                }

                NotifyTask = new NotifyTaskBase.Builder<PageResult<TResult>>(() => _pageSourceLoader(pageNumber, PageSize))
                   .WithWhenSuccessfullyCompleted(OnPageRetrieved)
                   .WithWhenFaulted(OnLoadingFaulted)
                   .WithWhenCompleted(_onTaskCompleted)
                   .WithDefaultResult(PageResult<TResult>.Empty)
                   .Build();

                NotifyTask.Start();
            }
        }

        private void OnPageRetrieved(PageResult<TResult> result)
        {
            Contract.Requires(() => NotifyTask != null);
            Contract.Requires(() => result != null);

            Log.Info($"On page retrieved callback {result.Items.Count} items retrieved, total remote items is {result.TotalCount}");            
            if (_refreshRequested)
            {
                Reset();
            }

            TotalCount = Math.Min(result.TotalCount, _maxItemCount);
            PageLoadedCount++;
            Items = Items.AddRange(result.Items);
            Log.Info($"{Items.Count} items in paginator collection, {PageLoadedCount} page loaded");

            Contract.Ensures(() => PageLoadedCount > 0);
            Contract.Ensures(() => result.Items != null && _maxItemCount >= 0);
        }

        private void OnLoadingFaulted(Exception exception)
        {
            Contract.Requires(() => NotifyTask != null);
            Contract.Requires(() => exception != null);

            Log.Info("Page loading task failed");
        }
    }
}