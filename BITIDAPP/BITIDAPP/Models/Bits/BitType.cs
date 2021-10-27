using BITIDAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace BITIDAPP.Models.Bits
{
    public class BitType : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков
        public List<BitTypeModel> Collection { get; set; }
        public List<BitTypeMLModel> MLCollection { get; set; }
        public ObservableCollection<BitTypeJoin> JoinCollection { get; set; }

        private BitTypeJoin _SelectedJoinItem;
        private BitTypeJoin _NewItem = new BitTypeJoin { };

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


        public BitType(string SearchCriterion)
        {
            readMode = true;

            Collection = App.Database.Table<BitTypeModel>().OrderBy(a => a.TYPENAME).ToListAsync().Result;

            MLCollection = App.Database.Table<BitTypeMLModel>().Where(a => a.LANGUAGE == App.AppLanguage).ToListAsync().Result;

            IEnumerable<BitTypeJoin> joinList =
            from collection in App.Database.Table<BitTypeModel>().ToListAsync().Result
            join mlCollection in App.Database.Table<BitTypeMLModel>().Where(a => a.LANGUAGE == App.AppLanguage).ToListAsync().Result on collection.TYPEID equals mlCollection.TYPEID into joinCollection
            from mlCollection in joinCollection.DefaultIfEmpty(new BitTypeMLModel { })
            select new BitTypeJoin
            {
                TYPEID = collection.TYPEID,
                TYPENAME = collection.TYPENAME,
                PICTURE = collection.PICTURE,
                SUBID = collection.TYPEID,
                DESCRIPTION = mlCollection.DESCRIPTION,
                NOTE = mlCollection.NOTE,
                LANGUAGE = mlCollection.LANGUAGE
            };

            JoinCollection = new ObservableCollection<BitTypeJoin>(joinList.Where(a =>
                a.TYPENAME.ToLowerInvariant().Contains(SearchCriterion) ||
                (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(SearchCriterion)) ||
                (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(SearchCriterion))).OrderBy(a => a.TYPENAME).ToList());

            // If the table is empty, initialize the collection
            if (!App.Database.Table<BitTypeModel>().ToListAsync().Result.Any())
            {
                JoinCollection?.Add(NewItem);
            }
        }

        // Удаляем текущую запись в основной коллекции
        public void DeleteItem()
        {
            try
            {
                lock (collisionLock)
                {
                    App.Database.DeleteAsync<BitTypeModel>(SelectedJoinItem.TYPEID);
                    JoinCollection.Remove(SelectedJoinItem);
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

        // Сохраняем или создаем запись в основной коллекции
        public void UpdateItem(BitTypeModel temp)
        {
            try
            {
                lock (collisionLock)
                {
                    if (SelectedJoinItem.TYPEID == 0)
                    {
                        App.Database.InsertAsync(temp);
                    }
                    else
                    {
                        App.Database.UpdateAsync(temp);
                    }
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

        // Сохраняем или создаем запись в подчиненной коллекции
        public void UpdateMLItem(BitTypeMLModel newMLItem, BitTypeMLModel selectMLItem)
        {
            try
            {
                lock (collisionLock)
                {
                    if (MLCollection.FirstOrDefault(mlc => mlc.TYPEID == SelectedJoinItem.TYPEID) == null)
                    {
                        App.Database.InsertAsync(newMLItem);
                    }
                    else
                    {
                        App.Database.UpdateAsync(selectMLItem);
                    }
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

        public BitTypeJoin SelectedJoinItem
        {
            get => _SelectedJoinItem;
            set
            {
                _SelectedJoinItem = value;
                OnPropertyChanged();
            }
        }

        public BitTypeJoin NewItem
        {
            get => _NewItem;
            set
            {
                _NewItem = value;
                OnPropertyChanged();
            }
        }
    }



    public class BitTypeJoin : ViewModelBase
    {
        // Catalog
        public int TYPEID { get; set; }   // Уникальный код группы

        public string TYPENAME   // Название номенклатурной группы
        {
            get => typename;
            set
            {
                typename = value;
                OnPropertyChanged(nameof(TYPENAME));
            }
        }

        public byte[] PICTURE
        {
            get => picture;
            set
            {
                picture = value;
                OnPropertyChanged(nameof(PICTURE));
            }
        }

        // Sub Catalog
        public int SUBID   // Уникальный код в подчиненной коллекции
        {
            get => typeid;
            set
            {
                typeid = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        public string DESCRIPTION   // Описание
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged(nameof(DESCRIPTION));
            }
        }

        public string NOTE   // Примечания
        {
            get => note;
            set
            {
                note = value;
                OnPropertyChanged(nameof(NOTE));
            }
        }

        public string LANGUAGE   // Язык
        {
            get => language;
            set
            {
                language = value;
                OnPropertyChanged(nameof(LANGUAGE));
            }
        }

        // Catalog
        public int typeid;
        public string typename;
        public byte[] picture;

        // Sub Catalog
        public string description;
        public string note;
        public string language;

        public BitTypeJoin()
        {
        }
    }


    //Таблица групп породоразрушающего инструмента
    [Table("tbBitType")]
    public class BitTypeModel : ViewModelBase
    {
        [Column("BitTypeID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int TYPEID { get; set; }   // Уникальный код группы

        [Column("BitType"), NotNull, Unique, Indexed]
        public string TYPENAME   // Название номенклатурной группы
        {
            get => typename.ToUpper();
            set
            {
                typename = value.ToUpper();
                OnPropertyChanged(nameof(TYPENAME));
            }
        }

        [Column("Picture")]
        public byte[] PICTURE
        {
            get => picture;
            set
            {
                picture = value;
                OnPropertyChanged(nameof(PICTURE));
            }
        }

        [OneToMany(CascadeOperations = CascadeOperation.All)]      // One to many relationship with Valuation
        public List<BitCodeModel> BitCode { get; set; }

        public int typeid;
        public List<BitCodeModel> bitCode;
        public string typename;
        public byte[] picture;

        public BitTypeModel()
        {
            //this.TYPENAME = null;
            //this.PICTURE = null;
        }
    }



    //Таблица групп породоразрушающего инструмента
    [Table("tbBitTypeML")]
    public class BitTypeMLModel : ViewModelBase
    {
        [Column("BitTypeMLID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int BITTYPEMLID { get; set; }   // Уникальный код

        [Column("BitTypeID"), NotNull, Indexed, ForeignKey(typeof(BitTypeModel))]     // Specify the foreign key
        public int TYPEID   // Уникальный код группы
        {
            get => typeid;
            set
            {
                typeid = value;
                OnPropertyChanged(nameof(TYPEID));
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

        [Column("Note")]
        public string NOTE   // Примечания
        {
            get => note;
            set
            {
                note = value;
                OnPropertyChanged(nameof(NOTE));
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



        public int bittypemlid;
        public int typeid;
        public string description;
        public string note;
        public string language;

        public BitTypeMLModel()
        {
        }
    }

}