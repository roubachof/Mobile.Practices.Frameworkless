using System;

using Android.Views;
using Android.Widget;

using MetroLog;

using SeLoger.Lab.Playground.Core;
using SeLoger.Lab.Playground.ViewModels;

using DisplayState = SeLoger.Lab.Playground.ViewModels.DisplayState;

namespace SeLoger.Lab.Playground.Droid.Components
{
    public class ViewModelStateProcessor
    {
        private static readonly ILogger Log = LoggerFactory.GetLogger(nameof(ViewModelStateProcessor));

        private readonly Action<ViewModelState> _updateResultView;

        private readonly ProgressBar _loadingView;

        private readonly ErrorViewSwitcher _errorViewSwitcher;

        private readonly View _resultView;

        public ViewModelStateProcessor(ProgressBar loadingView, ErrorViewSwitcher errorViewSwitcher, View resultView, Action<ViewModelState> updateResultView)
        {
            _resultView = resultView;
            _errorViewSwitcher = errorViewSwitcher;
            _loadingView = loadingView;
            _updateResultView = updateResultView;
        }

        public void Process(ViewModelState viewModelState)
        {
            Log.Info($"Processing view model state {viewModelState}");

            _loadingView.SetIsVisible(viewModelState.Display == DisplayState.Loading);

            if (_resultView.SetIsVisible(viewModelState.Display == DisplayState.Result))
            {
                _updateResultView(viewModelState);
            }

            if (_errorViewSwitcher.SetIsVisible(viewModelState.Display == DisplayState.Error))
            {
                _errorViewSwitcher.Switch(viewModelState.Error);
            }

            Log.Info(
                string.Format(
                    "End of ProcessViewModelState: loader {0}, refreshLayout {1}, error view {2}",
                    _loadingView.GetVisibilityDescription(),
                    _resultView.GetVisibilityDescription(),
                    _errorViewSwitcher.GetVisibilitDescription()));
        }
    }
}