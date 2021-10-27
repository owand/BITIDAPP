using System.Collections.Generic;
using System.Windows.Input;

namespace BITIDAPP.Models
{
    public abstract class ViewModelBase : ObservableProperty
    {
        public Dictionary<string, ICommand> Commands { get; protected set; }

        public ViewModelBase()
        {
            Commands = new Dictionary<string, ICommand>();
        }
    }
}
