using BITIDAPP.Services;
using System.Threading;

[assembly: Xamarin.Forms.Dependency(typeof(BITIDAPP.iOS.Services.CloseApp_iOS))]
namespace BITIDAPP.iOS.Services
{
    public class CloseApp_iOS : ICloseApplication
    {
        public void CloseApp()
        {
            Thread.CurrentThread.Abort();
        }
    }
}