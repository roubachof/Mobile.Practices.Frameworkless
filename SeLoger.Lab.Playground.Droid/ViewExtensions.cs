using Android.Views;

namespace SeLoger.Lab.Playground.Droid
{
    public static class ViewExtensions
    {
        public static bool IsVisible(this View view)
        {
            return view.Visibility == ViewStates.Visible;
        }

        public static bool SetIsVisible(this View view, bool value)
        {
            view.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
            return value;
        }
    }
}