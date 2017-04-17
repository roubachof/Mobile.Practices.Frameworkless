using System;

using Android.Content;
using Android.Views;
using Android.Widget;

using MetroLog;

using Mobile.Practices.Frameworkless.Core;
using Mobile.Practices.Frameworkless.ViewModels;

using SeLoger.Contracts;

namespace Mobile.Practices.Frameworkless.Droid.Components
{
    public class ErrorViewSwitcher
    {
        private static readonly ILogger Log = LoggerFactory.GetLogger(nameof(ErrorViewSwitcher));

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

        public string GetVisibilitDescription()
        {
            return _errorView.GetVisibilityDescription();
        }
        
        public void Switch(ErrorType errorType)
        {
            Contract.Requires(() => errorType != ErrorType.None);
            
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