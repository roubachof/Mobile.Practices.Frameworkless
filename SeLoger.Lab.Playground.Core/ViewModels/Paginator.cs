using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Conditions;

using MetroLog;

using SeLoger.Lab.Playground.Core.Nito;
using SeLoger.Lab.Playground.Core.Services;

namespace SeLoger.Lab.Playground.Core.ViewModels
{
    public class Paginator<TResult> : IPagedListProvider, IInfiniteListLoader
    {
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(nameof(Paginator<TResult>));

        private readonly object _syncRoot = new object();

        private readonly int _maxItemCount;

        private readonly Func<int, int, Task<PageResult<TResult>>> _pageSourceLoader;

        private readonly Action _onTaskCompleted;

        private Dictionary<int, Task> _pageLoadingTasks;

        public Paginator(int pageSize, int maxItemCount, Func<int, int, Task<PageResult<TResult>>> pageSourceLoader, Action onTaskCompleted)
        {
            Log.Info($"Building paginator with pageSize: {pageSize}, maxItemCount: {maxItemCount}");
            PageSize = pageSize;
            _maxItemCount = maxItemCount;
            _pageSourceLoader = pageSourceLoader;
            _onTaskCompleted = onTaskCompleted;
            Reset();
        }

        public int PageLoadedCount
        {
            get
            {
                lock (_syncRoot)
                {
                    return _pageLoadingTasks.Values.Count(task => task.Status == TaskStatus.RanToCompletion);
                }
            }
        } 
        
        public int LoadedCount => Items.Count;

        public bool IsFull => LoadedCount >= TotalCount;

        public int PageSize { get; }
        
        public int TotalCount { get; private set; }

        public bool HasStarted => NotifyTask != null;

        public NotifyTask<PageResult<TResult>> NotifyTask { get; private set; }

        public ImmutableList<TResult> Items { get; private set; }

        public void Reset()
        {
            Log.Info("Resetting paginator");
            _pageLoadingTasks = new Dictionary<int, Task>();
            Items = ImmutableList<TResult>.Empty;
            NotifyTask = null;
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
            pageNumber.Requires().IsGreaterThan(0);

            Log.Info($"Loading page n°{pageNumber}");
            lock(_syncRoot)
            {
                if (_pageLoadingTasks.ContainsKey(pageNumber))
                {
                    Log.Info($"The page n°{pageNumber} is currently loading or has already been loaded: returning");
                    return;
                }

                if (IsFull)
                {
                    Log.Info($"Cannot load page {pageNumber} max item count has already been reached ({_maxItemCount})");
                }

                NotifyTask = new NotifyTaskBase.Builder<PageResult<TResult>>(_pageSourceLoader(pageNumber, PageSize))
                   .WithWhenSuccessfullyCompleted(OnPageRetrieved)
                   .WithWhenFaulted(OnLoadingFaulted)
                   .WithWhenCompleted(_onTaskCompleted)
                   .Build();

                _pageLoadingTasks.Add(pageNumber, NotifyTask.Task);
            }
        }
        
        private void OnPageRetrieved(PageResult<TResult> result)
        {
            Log.Info($"On page retrieved callback {result.Items.Count} items retrieved, total items are {result.TotalCount}");
            TotalCount = Math.Min(result.TotalCount, _maxItemCount);
            Items = Items.AddRange(result.Items);
            Log.Info($"{Items.Count} items in paginator collection");
        }

        private void OnLoadingFaulted(Exception exception)
        {
            Log.Info("Page loading task failed");
        }
    }
}