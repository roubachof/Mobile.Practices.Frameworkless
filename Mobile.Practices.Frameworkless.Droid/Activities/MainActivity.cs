using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Widget;

using MetroLog;

using Mobile.Practices.Frameworkless.Core;
using Mobile.Practices.Frameworkless.Droid.Adapters;
using Mobile.Practices.Frameworkless.Droid.Components;
using Mobile.Practices.Frameworkless.ViewModels;

using RecyclerViewAnimators.Animators;

using SeLoger.Contracts;

using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Mobile.Practices.Frameworkless.Droid.Activities
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
                        
            InitializeViewModel();
            InitializeViews();

            _viewModel.Load();
        }

        private void InitializeViewModel()
        {
            Log.Info("InitializeViewModel");

            _viewModel = DependencyContainer.Instance.GetInstance<SillyPeopleViewModel>();

            _viewModel.TaskCompleted += ViewModelOnTaskCompleted;
        }

        private void InitializeViews()
        {
            Contract.Requires(() => _viewModel != null);

            Log.Info("InitializeViews");

            SetContentView(Resource.Layout.activity_main);

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
            _recyclerView.AddOnScrollListener(new InfiniteScrollListener(_viewModel.InfiniteListLoader));
            _refreshLayout.Refresh += RefreshLayoutRefresh;
            _errorViewSwitcher.ErrorButtonClicked += ErrorButtonOnClick;
        }

        protected override void OnPause()
        {
            Log.Info("OnPause");

            _adapter.ItemClicked -= AdapterOnItemClicked;
            _recyclerView.ClearOnScrollListeners();
            _refreshLayout.Refresh -= RefreshLayoutRefresh;
            _errorViewSwitcher.ErrorButtonClicked -= ErrorButtonOnClick;
            
            base.OnPause();
        }

        protected override void OnDestroy()
        {
            Log.Info("OnDestroy");

            _viewModel.TaskCompleted -= ViewModelOnTaskCompleted;

            base.OnDestroy();
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
                _adapter.Add(_viewModel.LastPageResult.Items, _viewModel.LastPageResult.TotalCount, isRefreshed);
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
            
            Intent intent = new Intent(this, typeof(SillyDudeDetailsActivity));                   
            intent.PutExtra("id", viewModel.Id);

            if ((int)Build.VERSION.SdkInt >= (int)BuildVersionCodes.Lollipop)
            {
                intent.PutExtra(SillyDudeDetailsActivity.TRANSITION_NAME, viewModel.ImageUrl);

                var imageView = (ImageView)sender;
                imageView.TransitionName = SillyDudeDetailsActivity.TRANSITION_NAME;

                ActivityOptionsCompat options =
                    ActivityOptionsCompat.MakeSceneTransitionAnimation(this, imageView, imageView.TransitionName);

                ActivityCompat.StartActivity(this, intent, options.ToBundle());
            }
            else
            {
                StartActivity(intent);
            }
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