using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

using Com.Bumptech.Glide;

using SeLoger.Lab.Playground.Droid.Components;

using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace SeLoger.Lab.Playground.Droid.Activities
{
    [Activity(Label = "SillyDudeDetailActivity")]
    public class SillyDudeDetailActivity : AppCompatActivity
    {
        public const string TRANSITION_NAME = "MainActivity:image";

        private ImageView _sillyDudeImage; 

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            int sillyDudeId = Intent.GetIntExtra("id", 0);
            string imageUrl = Intent.GetStringExtra(TRANSITION_NAME);

            // Init VM

            SetContentView(Resource.Layout.activity_silly_details);

            _sillyDudeImage = FindViewById<ImageView>(Resource.Id.image_silly_dude);

            ViewCompat.SetTransitionName(_sillyDudeImage, TRANSITION_NAME);
            Glide
                .With(this)
                .Load(imageUrl)
                .Into(_sillyDudeImage);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            CollapsingToolbarLayout collapsingToolbarLayout = FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsing_toolbar);
            collapsingToolbarLayout.SetTitle("Louis C.K.");
        }
    }
}