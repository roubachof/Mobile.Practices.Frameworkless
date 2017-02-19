using System;
using System.Collections.Generic;

using Android.Content;
using Android.Runtime;
using Android.Support.V7.Util;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

using Com.Bumptech.Glide;

using MetroLog;

using SeLoger.Lab.Playground.Core;
using SeLoger.Lab.Playground.Droid.Components;
using SeLoger.Lab.Playground.Utils;
using SeLoger.Lab.Playground.ViewModels;

using WeakEvent;

namespace SeLoger.Lab.Playground.Droid.Adapters
{
    public class SillyRecyclerAdapter : RecyclerView.Adapter
    {
        private static readonly ILogger Log = LoggerFactory.GetLogger(nameof(SillyRecyclerAdapter));

        private readonly List<SillyDudeItemViewModel> _data;

        private readonly WeakReference<Context> _contextReference;

        private readonly WeakEventSource<SillyDudeItemViewModel> _itemClickedSource = new WeakEventSource<SillyDudeItemViewModel>();

        private int _maxItems;

        public SillyRecyclerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public SillyRecyclerAdapter(Context context)
        {
            _contextReference = new WeakReference<Context>(context);
            _data = new List<SillyDudeItemViewModel>();
            _maxItems = int.MaxValue;
        }

        public event EventHandler<SillyDudeItemViewModel> ItemClicked
        {
            add { _itemClickedSource.Subscribe(value); }
            remove { _itemClickedSource.Unsubscribe(value); }
        }

        public override int ItemCount => !IsMaxCountReached ? _data.Count + 1 : _data.Count;

        public bool IsMaxCountReached => _data.Count >= _maxItems;

        public void Add(IReadOnlyList<SillyDudeItemViewModel> viewModels, int maxItems, bool isRefreshed)
        {
            Log.Info($"Adding {viewModels.Count} view models, {maxItems} max items, refreshed is {isRefreshed}");
            int previousCount = ItemCount;
            _maxItems = maxItems;

            if (isRefreshed)
            {
                SillyDudeDiffCallback diffCallback = new SillyDudeDiffCallback(_data, viewModels);
                DiffUtil.DiffResult diffResult = DiffUtil.CalculateDiff(diffCallback);

                _data.Clear();
                _data.AddRange(viewModels);

                diffResult.DispatchUpdatesTo(this);
                return;
            }

            _data.AddRange(viewModels);
            NotifyItemRangeInserted(previousCount, viewModels.Count);
        }

        public override int GetItemViewType(int position)
        {
            return (position == ItemCount - 1) && !IsMaxCountReached ? 0 : 1;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if (viewType == 0)
            {
                // Loading view
                View loadingView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_loading, parent, false);
                return new LoadingViewHolder(loadingView);
            }

            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_silly, parent, false);
            return new SillyViewHolder(itemView, OnClick);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder.ItemViewType == 0)
            {
                // Loading view: nothing to bind
                return;
            }

            var viewHolder = (SillyViewHolder)holder;
            viewHolder.Update(_contextReference, _data[position]);
        }

        private void OnClick(int position)
        {
            _itemClickedSource.Raise(this, _data[position]);
        }               
    }

    public class SillyDudeDiffCallback : DiffUtil.Callback
    {
        private readonly IList<SillyDudeItemViewModel> _oldList;
        private readonly IReadOnlyList<SillyDudeItemViewModel> _newList;

        public SillyDudeDiffCallback(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public SillyDudeDiffCallback(IList<SillyDudeItemViewModel> oldList, IReadOnlyList<SillyDudeItemViewModel> newList)
        {
            _oldList = oldList;
            _newList = newList;
        }

        public override bool AreContentsTheSame(int p0, int p1)
        {
            var oldItem = _oldList[p0];
            var newItem = _newList[p1];

            return oldItem.Role == newItem.Role && oldItem.Name == newItem.Name && oldItem.ImageUrl == newItem.ImageUrl;
        }

        public override bool AreItemsTheSame(int p0, int p1)
        {
            return _oldList[p0].Id == _newList[p1].Id;
        }

        public override int NewListSize => _newList.Count;

        public override int OldListSize => _oldList.Count;
    }

    public class LoadingViewHolder : RecyclerView.ViewHolder
    {
        public LoadingViewHolder(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public LoadingViewHolder(View itemView)
            : base(itemView)
        {
        }
    }

    public class SillyViewHolder : RecyclerView.ViewHolder
    { 
        private readonly ImageView _photoView;

        private readonly TextView _nameView;

        private readonly TextView _descriptionView;
        
        public SillyViewHolder(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public SillyViewHolder(View itemView, Action<int> onItemClicked)
            : base(itemView)
        {
            _photoView = itemView.FindViewById<ImageView>(Resource.Id.image_silly);
            _nameView = itemView.FindViewById<TextView>(Resource.Id.text_name);
            _descriptionView = itemView.FindViewById<TextView>(Resource.Id.text_description);

            this.SetAnyHandler(
                (e) => itemView.Click += e,
                (e) => itemView.Click -= e,
                (s, e) => onItemClicked(AdapterPosition));
        }

        public void Update(WeakReference<Context> contextReference, SillyDudeItemViewModel itemViewModel)
        {
            _nameView.Text = itemViewModel.Name;
            _descriptionView.Text = itemViewModel.Role;

            Context context;
            if (!string.IsNullOrWhiteSpace(itemViewModel.ImageUrl) && contextReference.TryGetTarget(out context))
            {
                Glide
                    .With(context)
                    .Load(itemViewModel.ImageUrl)
                    .BitmapTransform(new CropCircleTransformation(context))
                    .Placeholder(Resource.Drawable.silly_grey_48dp)
                    .Into(_photoView);
            }
        }
    }
}