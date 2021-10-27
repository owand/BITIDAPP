using BITIDAPP.Services;
using Windows.UI.Xaml;

[assembly: Xamarin.Forms.Dependency(typeof(BITIDAPP.UWP.Services.CloseApp_UWP))]
namespace BITIDAPP.UWP.Services
{
    public class CloseApp_UWP : ICloseApplication
    {
        public void CloseApp()
        {
            Application.Current.Exit();
        }
    }
}
