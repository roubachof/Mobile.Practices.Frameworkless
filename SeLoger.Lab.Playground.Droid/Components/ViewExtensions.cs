using Android.Views;

namespace SeLoger.Lab.Playground.Droid.Components
{
    public static class ViewExtensions
    {
        private const string VISIBLE = "is visible";
        private const string NOT_VISIBLE = "is not visible";

        public static bool IsVisible(this View view)
        {
            return view.Visibility == ViewStates.Visible;
        }

        public static bool SetIsVisible(this View view, bool value)
        {
            view.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
            return value;
        }

        public static string GetVisibilityDescription(this View view)
        {
            return view.IsVisible() ? VISIBLE : NOT_VISIBLE;
        }
    }
}