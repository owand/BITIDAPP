using SQLiteNetExtensionsAsync.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace BITIDAPP.Models.Bits
{
    public class BitDecode : ViewModelBase
    {
        public List<BitCodeModel> code1SymbolList;
        public List<BitCodeModel> Code1SymbolList
        {
            get => code1SymbolList;
            set
            {
                code1SymbolList = value;
                OnPropertyChanged(nameof(Code1SymbolList));
            }
        }

        public List<BitCodeModel> code2SymbolList;
        public List<BitCodeModel> Code2SymbolList
        {
            get => code2SymbolList;
            set
            {
                code2SymbolList = value;
                OnPropertyChanged(nameof(Code2SymbolList));
            }
        }

        public List<BitCodeModel> code3SymbolList;
        public List<BitCodeModel> Code3SymbolList
        {
            get => code3SymbolList;
            set
            {
                code3SymbolList = value;
                OnPropertyChanged(nameof(Code3SymbolList));
            }
        }

        public List<BitCodeModel> code4SymbolList;
        public List<BitCodeModel> Code4SymbolList
        {
            get => code4SymbolList;
            set
            {
                code4SymbolList = value;
                OnPropertyChanged(nameof(Code4SymbolList));
            }
        }

        public BitDecode(int Id)
        {
            code1SymbolList = App.Database.Table<BitCodeModel>().Where(c => c.TYPEID == Id && c.SERIAL == 1 && c.LANGUAGE == App.AppLanguage).ToListAsync().Result;
            code2SymbolList = App.Database.Table<BitCodeModel>().Where(c => c.TYPEID == Id && c.SERIAL == 2 && c.LANGUAGE == App.AppLanguage).ToListAsync().Result;
            code3SymbolList = App.Database.Table<BitCodeModel>().Where(c => c.TYPEID == Id && c.SERIAL == 3 && c.LANGUAGE == App.AppLanguage).ToListAsync().Result;
            code4SymbolList = App.Database.Table<BitCodeModel>().Where(c => c.TYPEID == Id && c.SERIAL == 4 && c.LANGUAGE == App.AppLanguage).ToListAsync().Result;
        }


        public List<BitCodeModel> DecodeContent(int TypeId, string Symbol_1, string Symbol_2, string Symbol_3, string Symbol_4)
        {
            List<BitCodeModel> CollectionList =
            App.Database.QueryAsync<BitCodeModel>($"SELECT DISTINCT tbBitCode.Feature, tbBitCode.Specification FROM tbBitCode " +
            $"WHERE (tbBitCode.BitTypeID LIKE '{TypeId}' " +
            $"AND (tbBitCode.BitCodeID LIKE '{Symbol_1}' OR tbBitCode.BitCodeID LIKE '{Symbol_2}' " +
            $"OR tbBitCode.BitCodeID LIKE '{Symbol_3}' OR tbBitCode.BitCodeID LIKE '{Symbol_4}')) ").Result.GroupBy(a => (a.FEATURE, a.SPECIFICATION)).Select(x => x.First()).OrderBy(a => a.SERIAL).ToList();

            return CollectionList;
        }
    }


    public class BitTypeListViewModel : ViewModelBase
    {
        public List<BitCodeModel> typePickerList;
        public List<BitCodeModel> TypePickerList
        {
            get => typePickerList;
            set
            {
                typePickerList = value;
                OnPropertyChanged(nameof(TypePickerList));
            }
        }

        public BitTypeListViewModel()
        {
            typePickerList = ReadOperations.GetAllWithChildrenAsync<BitCodeModel>(App.Database).Result.GroupBy(x => x.TYPEID).Select(x => x.First()).ToList();
        }
    }
}