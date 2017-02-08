using System;
using System.Diagnostics.CodeAnalysis;

using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

using SeLoger.Lab.Playground.Core.ViewModels;

namespace SeLoger.Lab.Playground.Droid
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class InfiniteScrollListener : RecyclerView.OnScrollListener
    {
        private readonly IInfiniteListLoader _infiniteListLoader;

        public InfiniteScrollListener(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public InfiniteScrollListener(IInfiniteListLoader infiniteListLoader)
        {
            _infiniteListLoader = infiniteListLoader;
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            base.OnScrolled(recyclerView, dx, dy);

            var layoutManager = (LinearLayoutManager)recyclerView.GetLayoutManager();
            int visibleItemCount = recyclerView.ChildCount;
            int firstVisibleItem = layoutManager.FindFirstVisibleItemPosition();

            _infiniteListLoader.OnScroll(firstVisibleItem + visibleItemCount - 1);
        }        
    }
}