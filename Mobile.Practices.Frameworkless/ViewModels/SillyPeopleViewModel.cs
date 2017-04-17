using System;
using System.Linq;
using System.Threading.Tasks;

using MetroLog;

using Mobile.Practices.Frameworkless.Core;
using Mobile.Practices.Frameworkless.InfiniteList;
using Mobile.Practices.Frameworkless.Services;
using Mobile.Practices.Frameworkless.ViewModels.Extensions;

using SeLoger.Contracts;

using WeakEvent;

namespace Mobile.Practices.Frameworkless.ViewModels
{
    public enum ViewModelState0
    {
        NotStarted = 0,
        Loading,
        LoadingMore,
        SuccessfullyLoaded,
        SuccessfullyLoadedNoResults,
        CommunicationError,
        UnhandledError
    }

    public class SillyPeopleViewModel
    {
        private static readonly ILogger Log = LoggerFactory.GetLogger(nameof(SillyPeopleViewModel));

        private const int PAGE_SIZE = 50;

        private const int MAX_ITEMS = 300;

        private readonly WeakEventSource<EventArgs> _taskCompletedSource;

        private readonly ISillyFrontService _sillyFrontService;

        /// <summary>
        /// Single Responsibility Principle
        /// </summary>
        private readonly Paginator<SillyDudeItemViewModel> _paginator;

        public SillyPeopleViewModel(ISillyFrontService sillyFrontService)
        {
            Log.Info("Constructing SillyPeopleViewModel");

            _sillyFrontService = sillyFrontService;

            _taskCompletedSource = new WeakEventSource<EventArgs>();
            _paginator = new Paginator<SillyDudeItemViewModel>(PAGE_SIZE, MAX_ITEMS, PaginatorDataSource, OnPaginatorTaskCompleted);
        }
        
        public event EventHandler<EventArgs> TaskCompleted
        {
            add => _taskCompletedSource.Subscribe(value);
            remove => _taskCompletedSource.Unsubscribe(value);
        }
        
        public string Title => $"{_paginator.LoadedCount} silly guys loaded";

        /// <summary>
        /// Law Of Demeter
        /// </summary>
        public IInfiniteListLoader InfiniteListLoader => _paginator;

        public PageResult<SillyDudeItemViewModel> LastPageResult 
            => _paginator.NotifyTask == null ? PageResult<SillyDudeItemViewModel>.Empty : _paginator.NotifyTask.Result;

        public ViewModelState GetState()
        {
            return _paginator.ToViewModelState();
        }

        /// Indirect workflow View => ViewModel => View
        public void Load()
        {
            Log.Info("Loading view model");
            _paginator.LoadPage(1);
        }
        
        private async Task<PageResult<SillyDudeItemViewModel>> PaginatorDataSource(int pageNumber, int pageSize)
        {
            var modelPageResult = await _sillyFrontService.GetSillyPeoplePage(pageNumber, pageSize);
            return new PageResult<SillyDudeItemViewModel>(
                modelPageResult.TotalCount,
                modelPageResult.Items.Select(model => new SillyDudeItemViewModel(model)).ToList());
        }
        
        private void OnPaginatorTaskCompleted()
        {
            Contract.Requires(() => _paginator.NotifyTask != null);

            Log.Info("OnPaginatorTaskCompleted");
            _taskCompletedSource.Raise(this, EventArgs.Empty);
        }
    }
}
