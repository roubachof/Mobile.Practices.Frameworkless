using System;
using Android.App;
using Android.Runtime;
using SeLoger.Mobile.ToolKit.Core;

namespace Mobile.Practices.Frameworkless.Droid
{
    public class FrameworklessApplication : ToolKitApplication
    {
        public FrameworklessApplication(IntPtr handle, JniHandleOwnership transer) 
            : base(handle, transer)
        {
        }
    }
}
