using System;
using System.Collections.Generic;
using System.Diagnostics;

using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Android.Support.V7.Widget;

using Com.Bumptech.Glide;

using SeLoger.Lab.Playground.Core.Utils;
using SeLoger.Lab.Playground.Core.ViewModels;

using WeakEvent;

namespace SeLoger.Lab.Playground.Droid
{
    public class SillyRecyclerAdapter : RecyclerView.Adapter
    {
        private readonly List<SillyDudeItemViewModel> _data;

        private readonly WeakReference<Context> _contextReference;

        private readonly WeakEventSource<SillyDudeItemViewModel> _itemClickedSource = new WeakEventSource<SillyDudeItemViewModel>();

        public SillyRecyclerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public SillyRecyclerAdapter(Context context)
        {
            _contextReference = new WeakReference<Context>(context);
            _data = new List<SillyDudeItemViewModel>();
        }

        public event EventHandler<SillyDudeItemViewModel> ItemClicked
        {
            add { _itemClickedSource.Subscribe(value); }
            remove { _itemClickedSource.Unsubscribe(value); }
        }

        public override int ItemCount => _data.Count;

        public void Add(IReadOnlyList<SillyDudeItemViewModel> viewModels)
        {
            int previousCount = _data.Count;
            _data.AddRange(viewModels);
            NotifyItemRangeInserted(previousCount, viewModels.Count);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_silly, parent, false);
            return new SillyViewHolder(itemView);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = (SillyViewHolder)holder;
            viewHolder.Update(_contextReference, _data[position]);
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
            _descriptionView.Text = itemViewModel.Name;

            Context context;
            if (!string.IsNullOrWhiteSpace(itemViewModel.ImageUrl) && contextReference.TryGetTarget(out context))
            {
                Glide.With(context).Load(itemViewModel.ImageUrl).Into(_photoView);
            }
        }
        

    }
}