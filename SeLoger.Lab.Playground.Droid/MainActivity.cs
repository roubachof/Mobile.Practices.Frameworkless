using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

using MetroLog;

using RecyclerViewAnimators.Animators;

using SeLoger.Lab.Playground.Core;
using SeLoger.Lab.Playground.Core.ViewModels;

using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace SeLoger.Lab.Playground.Droid
{
    [Activity]
    public class MainActivity : AppCompatActivity
    {
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(nameof(MainActivity));

        private Toolbar _toolbar;

        private RecyclerView _recyclerView;

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

            _toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            _toolbar.Title = _viewModel.Title;
            SetSupportActionBar(_toolbar);

            _loader = FindViewById<ProgressBar>(Resource.Id.progress_loading);

            _errorViewSwitcher = new ErrorViewSwitcher(this, FindViewById<LinearLayout>(Resource.Id.view_error));

            _adapter = new SillyRecyclerAdapter(this);
            _recyclerView = FindViewById<RecyclerView>(Resource.Id.list_silly);
            _recyclerView.SetItemAnimator(new ScaleInAnimator());
            _recyclerView.SetAdapter(_adapter);
            _recyclerView.SetLayoutManager(new LinearLayoutManager(this));
        }

        protected override void OnResume()
        {
            Log.Info("OnResume");
            base.OnResume();

            _adapter.ItemClicked += AdapterOnItemClicked;
            _recyclerView.AddOnScrollListener(new InfiniteScrollListener(_viewModel.Paginator));
        }

        protected override void OnPause()
        {
            Log.Info("OnPause");
            base.OnPause();

            _adapter.ItemClicked -= AdapterOnItemClicked;
            _recyclerView.ClearOnScrollListeners();
        }

        private void ViewModelOnTaskCompleted(object sender, EventArgs eventArgs)
        {
            Log.Info($"ViewModelOnTaskCompleted {_viewModel.State}");

            System.Diagnostics.Debug.Assert(_viewModel.State != ViewModelState.Loading 
                && _viewModel.State != ViewModelState.NotStarted
                && _viewModel.State != ViewModelState.LoadingMore);

            _toolbar.Title = _viewModel.Title;

            UpdateVisibilities(_viewModel.State);
            UpdateRecyclerView();
        }

        private void UpdateVisibilities(ViewModelState state)
        {
            _loader.Visibility = state == ViewModelState.Loading 
                ? ViewStates.Visible 
                : ViewStates.Gone;

            _recyclerView.Visibility = state == ViewModelState.SuccessfullyLoaded
                ? ViewStates.Visible
                : ViewStates.Gone;

            _errorViewSwitcher.Switch(state);
        }

        private void UpdateRecyclerView()
        {
            if (_viewModel.State == ViewModelState.SuccessfullyLoaded)
            {
                _adapter.Add(_viewModel.Paginator.NotifyTask.Result.Items, _viewModel.Paginator.NotifyTask.Result.TotalCount);
            }
            else
            {
                _adapter = new SillyRecyclerAdapter(this);
            }
        }

        private void AdapterOnItemClicked(object sender, SillyDudeItemViewModel viewModel)
        {
            Log.Info($"AdapterOnItemClicked item {viewModel}");
        }
    }

    public class ErrorViewSwitcher
    {
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
        
        public void Switch(ViewModelState viewModelState)
        {
            Context context;
            if (!_contextReference.TryGetTarget(out context))
            {
                return;
            }

            switch (viewModelState)
            {
                case ViewModelState.SuccessfullyLoadedNoResults:
                    _imageView.SetImageResource(Resource.Drawable.search_24dp);
                    _textView.Text = context.GetString(Resource.String.no_results);
                    _buttonView.Visibility = ViewStates.Gone;
                    break;

                case ViewModelState.CommunicationError:
                    _imageView.SetImageResource(Resource.Drawable.sad_cloud_24dp);
                    _textView.Text = context.GetString(Resource.String.error_network);
                    _buttonView.Visibility = ViewStates.Visible;
                    break;

                case ViewModelState.UnhandledError:
                    _imageView.SetImageResource(Resource.Drawable.bug_24dp);
                    _textView.Text = context.GetString(Resource.String.error_unknown);
                    _buttonView.Visibility = ViewStates.Visible;
                    break;

                default:
                    _errorView.Visibility = ViewStates.Gone;
                    return;
            }

            _errorView.Visibility = ViewStates.Visible;
        }
    }
}

