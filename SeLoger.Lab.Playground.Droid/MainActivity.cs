using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

using MetroLog;

using RecyclerViewAnimators.Animators;

using SeLoger.Lab.Playground.Core;
using SeLoger.Lab.Playground.Core.ViewModels;

using DisplayState = SeLoger.Lab.Playground.Core.ViewModels.DisplayState;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace SeLoger.Lab.Playground.Droid
{
    [Activity]
    public class MainActivity : AppCompatActivity
    {
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(nameof(MainActivity));

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

            _viewModel.Load();
            
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
            Log.Info($"ViewModelOnTaskCompleted");

            ViewModelState viewModelState = _viewModel.GetState();

            System.Diagnostics.Debug.Assert(viewModelState.Display != DisplayState.Loading);

            _toolbar.Title = _viewModel.Title;

            ProcessViewModelState(viewModelState);
        }

        private void RefreshLayoutRefresh(object sender, EventArgs e)
        {
            _refreshLayout.Refreshing = true;
            _viewModel.Load();
        }

        private void ProcessViewModelState(ViewModelState state)
        {
            Log.Info($"Processing view model state {state}");
            if (state.IsRefreshed)
            {
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
        }

        private void UpdateRecyclerView(ErrorType error, bool isRefreshed)
        {
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

    public class ErrorViewSwitcher
    {
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(nameof(ErrorViewSwitcher));

        private readonly WeakReference<Context> _contextReference;

        private readonly LinearLayout _errorView;

        private readonly ImageView _imageView;

        private readonly TextView _textView;

        private readonly Button _buttonView;

        public ErrorViewSwitcher(Context context, LinearLayout errorView)
        {
            _contextReference = new WeakReference<Context>(context);
            _errorView = errorView;

            _imageView = errorView.FindViewById<ImageView>(Resource.Id.image_error);
            _textView = errorView.FindViewById<TextView>(Resource.Id.text_error);
            _buttonView = errorView.FindViewById<Button>(Resource.Id.button_retry);
        }

        public event EventHandler ErrorButtonClicked
        {
            add { _buttonView.Click += value; }
            remove { _buttonView.Click -= value; }
        }
        
        public bool IsVisible()
        {
            return _errorView.IsVisible();
        }

        public bool SetIsVisible(bool value)
        {
            return _errorView.SetIsVisible(value);
        }
        
        public void Switch(ErrorType errorType)
        {
            Context context;
            if (!_contextReference.TryGetTarget(out context))
            {
                return;
            }

            switch (errorType)
            {
                case ErrorType.NoResults:
                    Log.Info("Switching to no results error view");
                    _imageView.SetImageResource(Resource.Drawable.search_24dp);
                    _textView.Text = context.GetString(Resource.String.no_results);
                    _buttonView.Visibility = ViewStates.Gone;
                    break;

                case ErrorType.Communication:
                    Log.Info("Switching to communication error view");
                    _imageView.SetImageResource(Resource.Drawable.sad_cloud_24dp);
                    _textView.Text = context.GetString(Resource.String.error_network);
                    _buttonView.Visibility = ViewStates.Visible;
                    break;

                case ErrorType.Unhandled:
                    Log.Info("Switching to unhandled error view");
                    _imageView.SetImageResource(Resource.Drawable.bug_24dp);
                    _textView.Text = context.GetString(Resource.String.error_unknown);
                    _buttonView.Visibility = ViewStates.Visible;
                    break;

                default:
                    _errorView.Visibility = ViewStates.Gone;
                    return;
            }
        }
    }
}

