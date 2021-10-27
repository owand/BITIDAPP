using BITIDAPP.Resources;
using BITIDAPP.Views.Settings;
using SQLite;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
//[assembly: ExportFont("MaterialIcons-Regular.ttf", Alias = "MaterialIcons")]
namespace BITIDAPP
{
    public partial class App : Application
    {
        // Переменные для базы данных
        public const string dbName = "BITDBCatalog.db";
        public const int dbVersion = 49;


        public static SQLiteAsyncConnection database;
        public static SQLiteAsyncConnection Database
        {
            get
            {
                if (database == null)
                {
                    // путь, по которому будет находиться база данных
                    string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), dbName);
                    //получаем текущую сборку
                    Assembly assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
                    Stream stream = assembly.GetManifestResourceStream($"BITIDAPP.{dbName}");

                    // если база данных не существует (еще не скопирована)
                    if (!File.Exists(dbPath))
                    {
                        //берем из нее ресурс базы данных и создаем из него поток
                        using (stream)
                        {
                            using (FileStream fs = new FileStream(dbPath, FileMode.OpenOrCreate))
                            {
                                stream.CopyTo(fs);  // копируем файл базы данных в нужное нам место
                                fs.Flush();
                            }
                        }
                    }
                    database = new SQLiteAsyncConnection(dbPath);

                    int currentDbVersion = database.ExecuteScalarAsync<int>("pragma user_version").Result;
                    if (currentDbVersion < dbVersion)
                    {
                        //берем из нее ресурс базы данных и создаем из него поток
                        using (stream)
                        {
                            using (FileStream fs = new FileStream(dbPath, FileMode.OpenOrCreate))
                            {
                                stream.CopyTo(fs);  // копируем файл базы данных в нужное нам место
                                fs.Flush();
                            }
                        }
                    }
                }
                return database;
            }
        }


        // Переменные для подключения приложения к личному account Microsoft, используя Microsoft Graph API
        public static string ClientID = "8c35c4df-7c9c-4357-b944-9ee4f4c59550";
        public static string[] Scopes = { "Files.ReadWrite.All", "Files.ReadWrite.AppFolder" };
        public static object ParentWindow { get; set; }

        //переменные для изменения локализации приложения
        public static string AppLanguage
        {
            get => Xamarin.Essentials.Preferences.Get("currentLanguage", "ru");
            set => Xamarin.Essentials.Preferences.Set("currentLanguage", value);
        }

        //переменные для применения темы
        public static string AppTheme
        {
            get => Xamarin.Essentials.Preferences.Get("currentTheme", "myOSTheme");
            set => Xamarin.Essentials.Preferences.Set("currentTheme", value);
        }

        //переменные для Purchases State
        public static bool AdStateCurrent
        {
            get => Xamarin.Essentials.Preferences.Get("AdStateCurrent", false);
            set => Xamarin.Essentials.Preferences.Set("AdStateCurrent", value);
        }

        public App()
        {
            Device.SetFlags(new string[] { "MediaElement_Experimental", "Shell_UWP_Experimental", "Visual_Experimental",
                                           "CollectionView_Experimental", "FastRenderers_Experimental", "CarouselView_Experimental",
                                           "IndicatorView_Experimental", "RadioButton_Experimental", "AppTheme_Experimental",
                                           "Markup_Experimental", "Expander_Experimental" });
            InitializeComponent();

            // Языковая культура приложения должна определяться как можно раньше.
            AppResource.Culture = new CultureInfo(AppLanguage);

            // Theme of the application
            switch (AppTheme)
            {
                case "myDarkTheme":
                    App.Current.UserAppTheme = OSAppTheme.Dark;
                    break;

                case "myLightTheme":
                    App.Current.UserAppTheme = OSAppTheme.Light;
                    break;

                case "myOSTheme":
                    App.Current.UserAppTheme = OSAppTheme.Unspecified;
                    break;

                default:
                    App.Current.UserAppTheme = OSAppTheme.Unspecified;
                    break;
            }

            MainPage = new AppShell();
        }


        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
