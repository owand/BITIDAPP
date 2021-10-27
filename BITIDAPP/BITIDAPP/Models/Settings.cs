using BITIDAPP.Resources;
using System.Collections.ObjectModel;
using System.Linq;

namespace BITIDAPP.Models
{
    public class Settings : ViewModelBase
    {
        public ObservableCollection<LangModel> LangCollection { get; }
        public ObservableCollection<ThemesModel> ThemesCollection { get; }

        public int AppLanguage { get; set; }
        public int AppTheme { get; set; }

        public Settings()
        {
            LangCollection = new ObservableCollection<LangModel>()
            {
                new LangModel { LANGDISPLAY = AppResource.LanguageRus, LANGNAME = "ru" },
                new LangModel { LANGDISPLAY = AppResource.LanguageEng, LANGNAME = "en" }
            };
            AppLanguage = LangCollection.IndexOf(LangCollection.Where(X => X.LANGNAME == App.AppLanguage).FirstOrDefault());

            ThemesCollection = new ObservableCollection<ThemesModel>()
            {
                new ThemesModel { THEMEDISPLAY =  AppResource.ThemesDarkName, THEMENAME = "myDarkTheme" },
                new ThemesModel { THEMEDISPLAY =  AppResource.ThemesLightName, THEMENAME = "myLightTheme" },
                new ThemesModel { THEMEDISPLAY =  AppResource.ThemesOSName, THEMENAME = "myOSTheme" }
            };
            AppTheme = ThemesCollection.IndexOf(ThemesCollection.Where(X => X.THEMENAME == App.AppTheme).FirstOrDefault());
        }
    }

    public class LangModel
    {
        public string LANGNAME { get; set; }

        public string LANGDISPLAY { get; set; }

        public LangModel()
        {
            LANGNAME = App.AppLanguage;
            //LANGDISPLAY = null;
        }
    }

    public class ThemesModel
    {
        public string THEMENAME { get; set; }

        public string THEMEDISPLAY { get; set; }

        public ThemesModel()
        {
            THEMENAME = App.AppTheme;
        }
    }
}