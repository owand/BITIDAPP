using BITIDAPP.Droid.Services;
using BITIDAPP.Services;

[assembly: Xamarin.Forms.Dependency(typeof(CloseApp_Droid))]
namespace BITIDAPP.Droid.Services
{
    public class CloseApp_Droid : ICloseApplication
    {
        public void CloseApp()
        {
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }
    }
}