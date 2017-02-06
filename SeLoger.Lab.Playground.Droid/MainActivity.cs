using Android.App;
using Android.OS;

using Android.Support.V7.Widget;

using RecyclerViewAnimators.Animators;

using SeLoger.Lab.Playground.Core;
using SeLoger.Lab.Playground.Core.ViewModels;

namespace SeLoger.Lab.Playground.Droid
{
    [Activity(Label = "SeLoger.Lab.Playground.Droid", MainLauncher = true, Icon = "@drawable/silly_96dp")]
    public class MainActivity : Activity
    {
        private RecyclerView _recyclerView;

        private SillyRecyclerAdapter _adapter;

        private SillyPeopleViewModel _viewModel;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _viewModel = DependencyContainer.Instance.GetInstance<SillyPeopleViewModel>();
            _viewModel.Load();
            
            SetContentView(Resource.Layout.main);

            _adapter = new SillyRecyclerAdapter(this);
            _recyclerView = FindViewById<RecyclerView>(Resource.Id.list_silly);
            _recyclerView.SetItemAnimator(new ScaleInAnimator());
            _recyclerView.SetAdapter(_adapter);


        }


    }
}

