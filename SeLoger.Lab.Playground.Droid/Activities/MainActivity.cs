using System;

using Android.App;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Widget;

using MetroLog;

using RecyclerViewAnimators.Animators;

using SeLoger.Contracts;
using SeLoger.Lab.Playground.Core;
using SeLoger.Lab.Playground.Droid.Adapters;
using SeLoger.Lab.Playground.Droid.Components;
using SeLoger.Lab.Playground.ViewModels;

using DisplayState = SeLoger.Lab.Playground.ViewModels.DisplayState;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace SeLoger.Lab.Playground.Droid.Activities
{
    [Activity]
    public class MainActivity : AppCompatActivity
    {
        private static readonly ILogger Log = LoggerFactory.GetLogger(nameof(MainActivity));

        private Toolbar _toolbar;

        private RecyclerView _recyclerView;

        private SwipeRefreshLayout _refreshLayout;

        private ProgressBar _loader;

        private ErrorViewSwitcher _errorViewSwitcher;
        
        private SillyRecyclerAdapter _adapter;
        
        private SillyPeopleViewModel _viewModel;

        protected override void OnCreate(Bundle bundle)
        {
            Log.Info("OnCreate");
            base.OnCreate(bundle);
            
            _viewModel = DependencyContainer.Instance.GetInstance<SillyPeopleViewModel>();

            // No need to unsubscribe => weakevent
            _viewModel.TaskCompleted += ViewModelOnTaskCompleted;

            CreateViews();

            _viewModel.Load();
        }

        private void CreateViews()
        {
            Contract.Requires(() => _viewModel != null);

            SetContentView(Resource.Layout.main);

            _toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            _toolbar.Title = _viewModel.Title;
            SetSupportActionBar(_toolbar);

            _loader = FindViewById<ProgressBar>(Resource.Id.progress_loading);

            _errorViewSwitcher = new ErrorViewSwitcher(
                this,
                FindViewById<LinearLayout>(Resource.Id.view_error));

            _adapter = new SillyRecyclerAdapter(this);
            _recyclerView = FindViewById<RecyclerView>(Resource.Id.list_silly);
            _recyclerView.SetItemAnimator(new ScaleInAnimator());
            _recyclerView.SetAdapter(_adapter);
            _recyclerView.SetLayoutManager(new LinearLayoutManager(this));

            _refreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.refresh_container);
            _refreshLayout.SetColorSchemeResources(Resource.Color.accent, Resource.Color.primary);
        }

        protected override void OnResume()
        {
            Log.Info("OnResume");
            base.OnResume();

            _adapter.ItemClicked += AdapterOnItemClicked;
            _recyclerView.AddOnScrollListener(new InfiniteScrollListener(_viewModel.Paginator));
            _refreshLayout.Refresh += RefreshLayoutRefresh;
            _errorViewSwitcher.ErrorButtonClicked += ErrorButtonOnClick;
        }

        protected override void OnPause()
        {
            Log.Info("OnPause");
            base.OnPause();

            _adapter.ItemClicked -= AdapterOnItemClicked;
            _recyclerView.ClearOnScrollListeners();
            _refreshLayout.Refresh -= RefreshLayoutRefresh;
            _errorViewSwitcher.ErrorButtonClicked -= ErrorButtonOnClick;
        }

        private void ViewModelOnTaskCompleted(object sender, EventArgs eventArgs)
        {
            Log.Info("ViewModelOnTaskCompleted");

            ViewModelState viewModelState = _viewModel.GetState();

            Contract.Requires(() => viewModelState.Display != DisplayState.Loading);

            _toolbar.Title = _viewModel.Title;

            ProcessViewModelState(viewModelState);
        }

        private void RefreshLayoutRefresh(object sender, EventArgs e)
        {
            Log.Info("RefreshLayoutRefresh");
            _refreshLayout.Refreshing = true;
            _viewModel.Load();
        }

        private void ProcessViewModelState(ViewModelState state)
        {
            Log.Info($"Processing view model state {state}");
            if (state.IsRefreshed)
            {
                Log.Info("Refreshing set to false");
                _refreshLayout.Refreshing = false;
            }

            _loader.SetIsVisible(state.Display == DisplayState.Loading);

            if (_refreshLayout.SetIsVisible(state.Display == DisplayState.Result))
            {
                UpdateRecyclerView(state.Error, state.IsRefreshed);
            }

            if (_errorViewSwitcher.SetIsVisible(state.Display == DisplayState.Error))
            {
                _errorViewSwitcher.Switch(state.Error);
            }

            Log.Info(
                string.Format(
                    "End of ProcessViewModelState: loader {0}, refreshLayout {1}, error view {2}",
                    _loader.GetVisibilityDescription(),
                    _refreshLayout.GetVisibilityDescription(),
                    _errorViewSwitcher.GetVisibilitDescription()));
        }

        private void UpdateRecyclerView(ErrorType error, bool isRefreshed)
        {
            Log.Info("UpdateRecyclerView");
            if (error == ErrorType.None)
            {
                _adapter.Add(_viewModel.Paginator.NotifyTask.Result.Items, _viewModel.Paginator.NotifyTask.Result.TotalCount, isRefreshed);
                if (isRefreshed)
                {
                    _recyclerView.ScrollToPosition(0);
                }

                return;
            }

            Toast.MakeText(this, ToErrorMessage(error), ToastLength.Short)
                 .Show();
        }

        private void AdapterOnItemClicked(object sender, SillyDudeItemViewModel viewModel)
        {
            Log.Info($"AdapterOnItemClicked item {viewModel}");
        }
   
        private void ErrorButtonOnClick(object sender, EventArgs eventArgs)
        {
            Log.Info("ErrorButtonOnClick");
            _viewModel.Load();
            ProcessViewModelState(_viewModel.GetState());
        }

        private string ToErrorMessage(ErrorType errorType)
        {
            switch (errorType)
            {
                case ErrorType.None:
                    return "Aucune erreur";
                case ErrorType.Communication:
                    return GetString(Resource.String.error_network);
                case ErrorType.Unhandled:
                    return GetString(Resource.String.error_unknown);
                case ErrorType.NoResults:
                    return GetString(Resource.String.no_results);
                default:
                    throw new InvalidOperationException($"Unhandled errortype ({errorType})");
            }
        }
    }
}