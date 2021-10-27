using BITIDAPP.Resources;
using SQLite;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace BITIDAPP.Models.Bits
{
    public class BitOD : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков
        public ObservableCollection<BitODModel> Collection { get; set; }

        private BitODModel _SelectedJoinItem;
        private BitODModel _NewItem = new BitODModel { };

        public bool readMode;
        public bool ReadMode
        {
            get => readMode;
            set
            {
                readMode = value;
                OnPropertyChanged(nameof(ReadMode));
            }
        }


        public BitOD(string SearchCriterion)
        {
            readMode = true;

            Collection = new ObservableCollection<BitODModel>(App.Database.Table<BitODModel>().ToListAsync().Result.Select(a => a).Where(a =>
                         (!string.IsNullOrEmpty(a.BITODNAME) && a.BITODNAME.ToLowerInvariant().Contains(SearchCriterion)) ||
                         (!string.IsNullOrEmpty(a.BITODINCH) && a.BITODINCH.ToLowerInvariant().Contains(SearchCriterion)) ||
                         (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(SearchCriterion))).OrderBy(a => a.BITOD).ToList());

            // If the table is empty, initialize the collection
            if (!App.Database.Table<BitODModel>().ToListAsync().Result.Any())
            {
                Collection?.Add(NewItem);
            }
        }

        // Сохраняем или создаем и сохраняем новую запись.
        public void UpdateItem(BitODModel temp)
        {
            try
            {
                lock (collisionLock)
                {
                    if (temp.BITODID == 0)
                    {
                        App.Database.InsertAsync(temp);
                    }
                    else
                    {
                        App.Database.UpdateAsync(temp);
                    }
                    App.Database.CloseAsync();
                }
            }
            catch (SQLiteException ex)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                });
                return;
            }
        }

        // Удаляем текущую запись.
        public void DeleteItem()
        {
            try
            {
                lock (collisionLock)
                {
                    App.Database.DeleteAsync<BitODModel>(SelectedJoinItem.BITODID);
                    Collection.Remove(SelectedJoinItem);
                }
            }
            catch (SQLiteException ex)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                });
                return;
            }
        }

        public BitODModel SelectedJoinItem
        {
            get => _SelectedJoinItem;
            set
            {
                _SelectedJoinItem = value;
                OnPropertyChanged();
            }
        }

        public BitODModel NewItem
        {
            get => _NewItem;
            set
            {
                _NewItem = value;
                OnPropertyChanged();
            }
        }
    }




    //Таблица диаметров породоразрушающего инструмента
    [Table("tbBitOD")]
    public class BitODModel : ViewModelBase
    {
        [Column("BitODID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int BITODID { get; set; }   // Уникальный код группы

        [Column("BitOD"), NotNull, Unique, Indexed]
        public decimal BITOD   // Название номенклатурной группы
        {
            get => bitod;
            set
            {
                bitod = value;
                OnPropertyChanged(nameof(BITOD));
            }
        }

        [Column("BitODinch"), NotNull]
        public string BITODINCH   // Наружный диаметр труб, дюйм
        {
            get => bitodinch;
            set
            {
                bitodinch = value;
                OnPropertyChanged(nameof(BITODINCH));
            }
        }

        [Column("Description")]
        public string DESCRIPTION
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged(nameof(DESCRIPTION));
            }
        }

        //public string BITODNAME => bitod.ToString("N2"); // Поле в американском формате
        public string BITODNAME => string.Format("{0:N2}", bitod); // Поле в американском формате

        public int bitodid;
        public decimal bitod;
        public string bitodinch;
        public string description;

        public BitODModel()
        {
        }
    }
}