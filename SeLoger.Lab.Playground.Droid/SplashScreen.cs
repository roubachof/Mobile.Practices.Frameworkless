// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SplashScreen.cs" company="The Silly Company">
//   The Silly Company 2016. All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;

namespace SeLoger.Lab.Playground.Droid
{
    [Activity(
        MainLauncher = true,
        NoHistory = true,
        Icon = "@drawable/silly_96dp",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashScreen : AppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(bundle);
            SetContentView(LayoutInflater.Inflate(Resource.Layout.splash, (ViewGroup)null));
        }

        protected override async void OnResume()
        {
            base.OnResume();

            try
            {
                await
                    DroidEntryPointSingleton.EnsureInitialized(
                        () => RunOnUiThread(() => StartActivity(typeof(MainActivity))));
            }
            catch (Exception exception)
            {
                Log.Error("silly", $"Exception while initializing app: {exception.Message}");
                throw;
            }
        }
    }
}