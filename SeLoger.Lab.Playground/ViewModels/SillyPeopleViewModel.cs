﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using MetroLog;

using SeLoger.Contracts;
using SeLoger.Lab.Playground.Core;
using SeLoger.Lab.Playground.InfiniteList;
using SeLoger.Lab.Playground.Services;
using SeLoger.Lab.Playground.ViewModels.Extensions;

using WeakEvent;

namespace SeLoger.Lab.Playground.ViewModels
{
    public enum ViewModelState2
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

        private readonly WeakEventSource<EventArgs> _taskCompletedSource = new WeakEventSource<EventArgs>();

        private readonly ISillyFrontService _sillyFrontService;

        public SillyPeopleViewModel(ISillyFrontService sillyFrontService)
        {
            _sillyFrontService = sillyFrontService;

            Paginator = new Paginator<SillyDudeItemViewModel>(PAGE_SIZE, MAX_ITEMS, PaginatorDataSource, OnPaginatorTaskCompleted);
        }
        
        public event EventHandler<EventArgs> TaskCompleted
        {
            add { _taskCompletedSource.Subscribe(value); }
            remove { _taskCompletedSource.Unsubscribe(value); }
        }
        
        public string Title => $"{Paginator.LoadedCount} silly guys loaded";

        public Paginator<SillyDudeItemViewModel> Paginator { get; }

        public ViewModelState GetState()
        {
            return Paginator.ToViewModelState();
        }

        public void Load()
        {
            Log.Info("Loading view model");
            Paginator.LoadPage(1);
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
            Contract.Requires(() => Paginator.NotifyTask != null);

            Log.Info("OnPaginatorTaskCompleted");
            _taskCompletedSource.Raise(this, EventArgs.Empty);
        }
    }
}