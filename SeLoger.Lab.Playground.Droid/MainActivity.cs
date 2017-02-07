using System;

using Android.App;
using Android.OS;

using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

using Com.Bumptech.Glide.Load.Model;

using MetroLog;

using RecyclerViewAnimators.Animators;

using SeLoger.Lab.Playground.Core;
using SeLoger.Lab.Playground.Core.ViewModels;

namespace SeLoger.Lab.Playground.Droid
{
    [Activity(Label = "SeLoger.Lab.Playground.Droid", MainLauncher = true, Icon = "@drawable/silly_96dp")]
    public class MainActivity : Activity
    {
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(nameof(MainActivity));

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

            _loader = FindViewById<ProgressBar>(Resource.Id.progress_loading);

            _errorViewSwitcher = new ErrorViewSwitcher(FindViewById<LinearLayout>(Resource.Id.view_error));

            _adapter = new SillyRecyclerAdapter(this);
            _recyclerView = FindViewById<RecyclerView>(Resource.Id.list_silly);
            _recyclerView.SetItemAnimator(new ScaleInAnimator());
            _recyclerView.SetAdapter(_adapter);
        }

        protected override void OnResume()
        {
            Log.Info("OnResume");
            base.OnResume();

            _adapter.ItemClicked += AdapterOnItemClicked;
        }

        protected override void OnPause()
        {
            Log.Info("OnPause");
            base.OnPause();

            _adapter.ItemClicked -= AdapterOnItemClicked;
        }

        private void ViewModelOnTaskCompleted(object sender, EventArgs eventArgs)
        {
            Log.Info($"ViewModelOnTaskCompleted {_viewModel.State}");

            _loader.Visibility = ViewStates.Gone;

            _recyclerView.Visibility = _viewModel.Paginator.NotifyTask.IsSuccessfullyCompleted
                                           ? ViewStates.Visible
                                           : ViewStates.Gone;

            _errorViewSwitcher.Visibility = _recyclerView.Visibility == ViewStates.Visible
                                                ? ViewStates.Gone
                                                : ViewStates.Visible;
        }

        private void AdapterOnItemClicked(object sender, SillyDudeItemViewModel viewModel)
        {
            Log.Info($"AdapterOnItemClicked item {viewModel}");
        }
    }

    public class ErrorViewSwitcher
    {
        private readonly LinearLayout _errorView;

        private readonly ImageView _imageView;

        private readonly TextView _textView;

        private readonly Button _buttonView;

        public ErrorViewSwitcher(LinearLayout errorView)
        {
            _errorView = errorView;

            _imageView = errorView.FindViewById<ImageView>(Resource.Id.image_error);
            _textView = errorView.FindViewById<TextView>(Resource.Id.text_error);
            _buttonView = errorView.FindViewById<Button>(Resource.Id.button_retry);
        }

        public ViewStates Visibility
        {
            get { return _errorView.Visibility; }
            set { _errorView.Visibility = value; }
        }

        public void SwitchFrom(ViewModelState viewModelState)
        {
            switch (viewModelState)
            {
                case ViewModelState.SuccessfullyLoadedNoResults:
                    _imageView.SetImageResource(Resource.Drawable.search_24dp);
                    _textView.Text = Resource.String.no_results;
                    _buttonView.Visibility = ViewStates.Gone;
                    break;
                case ViewModelState.CommunicationError:
                    _imageView.SetImageResource(Resource.Drawable.sad_cloud_24dp);
                    _textView.Text = Resource.String.error_network;
                    _buttonView.Visibility = ViewStates.Visible;
                    break;
                case ViewModelState.UnhandledError:
                    _imageView.SetImageResource(Resource.Drawable.bug_24dp);
                    _textView.Text = Resource.String.error_unknown;
                    _buttonView.Visibility = ViewStates.Visible;
                    break;
                default:
                    break;
            }
        }
    }
}

