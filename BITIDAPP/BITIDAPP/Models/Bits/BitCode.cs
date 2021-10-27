using BITIDAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensionsAsync.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace BITIDAPP.Models.Bits
{
    public class BitCode : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков
        public ObservableCollection<BitCodeModel> collection;
        public ObservableCollection<BitCodeModel> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged(nameof(Collection));
            }
        }
        public List<BitTypeModel> TypePickerList { get; set; }
        public List<SerialModel> SerialList { get; set; }

        private BitCodeModel _SelectedJoinItem;
        private BitCodeModel _NewItem = new BitCodeModel { };

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

        public bool detailMode;
        public bool DetailMode
        {
            get => detailMode;
            set
            {
                detailMode = value;
                OnPropertyChanged(nameof(DetailMode));
            }
        }


        public BitCode(string FilterCriterion, string SearchCriterion)
        {
            readMode = true;

            SerialList = new List<SerialModel> { new SerialModel {SERIALS = 1},
                                                 new SerialModel {SERIALS = 2},
                                                 new SerialModel {SERIALS = 3},
                                                 new SerialModel {SERIALS = 4} };

            TypePickerList = App.Database.Table<BitTypeModel>().OrderBy(a => a.TYPENAME).ToListAsync().Result;

            collection = new ObservableCollection<BitCodeModel>(App.Database.GetAllWithChildrenAsync<BitCodeModel>().Result.Select(a => a).Where(a =>
            a.LANGUAGE.Equals(App.AppLanguage) &&
            (string.IsNullOrEmpty(FilterCriterion) || a.TYPEID.ToString().Equals(FilterCriterion)) &&
            (a.SYMBOL.ToLowerInvariant().Contains(SearchCriterion) ||
            a.FEATURE.ToLowerInvariant().Contains(SearchCriterion) ||
            a.SPECIFICATION.ToLowerInvariant().Contains(SearchCriterion))).OrderBy(a => a.TYPEID));

            // If the table is empty, initialize the collection
            if (!App.Database.Table<BitCodeModel>().ToListAsync().Result.Any())
            {
                Collection?.Add(NewItem);
            }
        }

        // Сохраняем или создаем и сохраняем новую запись.
        public void UpdateItem(BitCodeModel temp)
        {
            try
            {
                lock (collisionLock)
                {
                    if (temp.BITCODEID == 0)
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
                    App.Database.DeleteAsync<BitCodeModel>(SelectedJoinItem.BITCODEID);
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

        public BitCodeModel SelectedJoinItem
        {
            get => _SelectedJoinItem;
            set
            {
                _SelectedJoinItem = value;
                OnPropertyChanged();
            }
        }

        public BitCodeModel NewItem
        {
            get => _NewItem;
            set
            {
                _NewItem = value;
                OnPropertyChanged();
            }
        }
    }

    public class SerialModel
    {
        public int SERIALS { get; set; }

        public SerialModel()
        {
        }
    }




    //Таблица словаря кодов породоразрушающего инструмента
    [Table("tbBitCode")]
    public class BitCodeModel : ViewModelBase
    {
        [Column("BitCodeID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int BITCODEID { get; set; }

        [Column("BitTypeID"), NotNull, Indexed, ForeignKey(typeof(BitTypeModel))]     // Specify the foreign key
        public int TYPEID
        {
            get => typeId;
            set
            {
                typeId = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        [Column("Serial"), NotNull]
        public int SERIAL
        {
            get => serial;
            set
            {
                serial = value;
                OnPropertyChanged(nameof(SERIAL));
            }
        }

        [Column("Symbol"), NotNull]
        public string SYMBOL
        {
            get => symbol;
            set
            {
                symbol = value;
                OnPropertyChanged(nameof(SYMBOL));
            }
        }

        [Column("Feature"), NotNull]
        public string FEATURE
        {
            get => feature;
            set
            {
                feature = value;
                OnPropertyChanged(nameof(FEATURE));
            }
        }

        [Column("Specification"), NotNull]
        public string SPECIFICATION
        {
            get => specification;
            set
            {
                specification = value;
                OnPropertyChanged(nameof(SPECIFICATION));
            }
        }

        [Column("Language"), NotNull, Indexed]
        public string LANGUAGE   // Язык
        {
            get => language;
            set
            {
                language = value;
                OnPropertyChanged(nameof(LANGUAGE));
            }
        }

        [ManyToOne]      // Many to one relationship with BitGroups
        public BitTypeModel BitType { get; set; }



        public int bitcodeid;
        public int typeId;
        public int serial;
        public string symbol;
        public string feature;
        public string specification;
        public string language;

        public BitCodeModel()
        {
        }
    }
}