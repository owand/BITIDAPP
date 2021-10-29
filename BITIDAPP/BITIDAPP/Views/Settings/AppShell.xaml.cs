using BITIDAPP.Views.BHA;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BITIDAPP.Views.Settings
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell : Shell
    {
        public Command GoBitTypeCommand { get; private set; }
        public Command GoBitODCommand { get; private set; }
        public Command GoBitCodeCommand { get; private set; }
        public Command GoToSettingsCommand { get; private set; }


        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(BitDecodePage), typeof(BitDecodePage));
            Routing.RegisterRoute(nameof(BitTypePage), typeof(BitTypePage));
            Routing.RegisterRoute(nameof(BitODPage), typeof(BitODPage));
            Routing.RegisterRoute(nameof(BitCodePage), typeof(BitCodePage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));

            GoBitTypeCommand = new Command(async () => { await Shell.Current.GoToAsync(nameof(BitTypePage)); Shell.Current.FlyoutIsPresented = false; });
            GoBitODCommand = new Command(async () => { await Shell.Current.GoToAsync(nameof(BitODPage)); Shell.Current.FlyoutIsPresented = false; });
            GoBitCodeCommand = new Command(async () => { await Shell.Current.GoToAsync(nameof(BitCodePage)); Shell.Current.FlyoutIsPresented = false; });
            GoToSettingsCommand = new Command(async () => { await Shell.Current.GoToAsync(nameof(SettingsPage)); Shell.Current.FlyoutIsPresented = false; });

            BindingContext = this;
        }


        //private void OpenSettings(object sender, System.EventArgs e)
        //{
        //    try
        //    {
        //        Device.BeginInvokeOnMainThread(async () =>
        //        {
        //            await Shell.Current.Navigation.PushAsync(new SettingsPage(), false);
        //            Shell.Current.FlyoutIsPresented = false;
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Что-то пошло не так
        //        Device.BeginInvokeOnMainThread(async () =>
        //        {
        //            await Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
        //        });
        //        return;
        //    }
        //}


    }
}