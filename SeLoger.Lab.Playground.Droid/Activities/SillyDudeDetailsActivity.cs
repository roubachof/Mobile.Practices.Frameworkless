using System;

using Android.App;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

using Com.Bumptech.Glide;

using MetroLog;

using SeLoger.Contracts;
using SeLoger.Lab.Playground.Core;
using SeLoger.Lab.Playground.Droid.Components;
using SeLoger.Lab.Playground.ViewModels;

using DisplayState = SeLoger.Lab.Playground.ViewModels.DisplayState;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace SeLoger.Lab.Playground.Droid.Activities
{
    [Activity]
    public class SillyDudeDetailsActivity : AppCompatActivity
    {
        private static readonly ILogger Log = LoggerFactory.GetLogger(nameof(SillyDudeDetailsActivity));

        public const string TRANSITION_NAME = "photo";

        private ImageView _sillyDudeImage;

        private SillyDudeDetailsViewModel _viewModel;

        private ViewModelStateProcessor _viewModelStateProcessor;

        private ErrorViewSwitcher _errorViewSwitcher;

        private TextView _roleTextView;

        private TextView _descriptionTextView;

        public override bool OnSupportNavigateUp()
        {
            SupportFinishAfterTransition();
            return true;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Info("OnCreate");

            base.OnCreate(savedInstanceState);

            InitializeViewModel();
            InitializeViews();
            
            _viewModel.Load(_viewModelStateProcessor.Process);
        }

        protected override void OnResume()
        {
            Log.Info("OnResume");

            base.OnResume();

            _errorViewSwitcher.ErrorButtonClicked += ErrorButtonOnClick;
        }

        protected override void OnPause()
        {
            Log.Info("OnPause");

            _errorViewSwitcher.ErrorButtonClicked -= ErrorButtonOnClick;

            base.OnPause();
        }

        private void ErrorButtonOnClick(object sender, EventArgs e)
        {
            Log.Info("ErrorButtonOnClick");

            _viewModel.Load(_viewModelStateProcessor.Process);
            _viewModelStateProcessor.Process(_viewModel.GetState());
        }

        private void InitializeViewModel()
        {
            Log.Info("InitializeViewModel");

            int sillyDudeId = Intent.GetIntExtra("id", 0);

            _viewModel = DependencyContainer.Instance.GetInstance<SillyDudeDetailsViewModel>();
            _viewModel.Initialize(sillyDudeId);
        }

        private void InitializeViews()
        {
            Log.Info("InitializeViews");

            SetContentView(Resource.Layout.activity_silly_details);

            _sillyDudeImage = FindViewById<ImageView>(Resource.Id.image_silly_dude);

            string imageUrl = Intent.GetStringExtra(TRANSITION_NAME);

            SupportPostponeEnterTransition();
            _sillyDudeImage.ViewTreeObserver.AddOnPreDrawListener(new PreDrawListener(_sillyDudeImage, SupportStartPostponedEnterTransition));
            

            var glideRequest = Glide.With(this);

            if (imageUrl.StartsWith("file"))
            {
                glideRequest.Load(Android.Net.Uri.Parse(imageUrl)).Into(_sillyDudeImage);
            }
            else
            {
                glideRequest.Load(imageUrl).Into(_sillyDudeImage);
            }
            
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            var loader = FindViewById<ProgressBar>(Resource.Id.progress_loading);

            _errorViewSwitcher = new ErrorViewSwitcher(
                this,
                FindViewById<LinearLayout>(Resource.Id.view_error));

            var detailsLayout = FindViewById<LinearLayout>(Resource.Id.layout_details);

            _viewModelStateProcessor = new ViewModelStateProcessor(loader, _errorViewSwitcher, detailsLayout, UpdateDetails);

            _roleTextView = detailsLayout.FindViewById<TextView>(Resource.Id.text_role);
            _descriptionTextView = detailsLayout.FindViewById<TextView>(Resource.Id.text_description);
        }

        private void UpdateDetails(ViewModelState state)
        {
            Contract.Requires(() => state.Display == DisplayState.Result && state.Error == ErrorType.None);

            CollapsingToolbarLayout collapsingToolbarLayout = FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsing_toolbar);
            collapsingToolbarLayout.SetTitle(_viewModel.Name);

            _roleTextView.Text = _viewModel.Role;
            _descriptionTextView.Text = _viewModel.Description;
        }

        private class PreDrawListener : Java.Lang.Object, ViewTreeObserver.IOnPreDrawListener
        {
            private readonly View _sharedView;

            private readonly Action _onPreDraw;

            public PreDrawListener(IntPtr handle, JniHandleOwnership transfer)
                : base(handle, transfer)
            {
            }

            public PreDrawListener(View sharedView, Action onPreDraw)
            {
                _sharedView = sharedView;
                _onPreDraw = onPreDraw;
            }

            public bool OnPreDraw()
            {
                _sharedView.ViewTreeObserver.RemoveOnPreDrawListener(this);
                _onPreDraw();
                return true;
            }
        }
    }
}
