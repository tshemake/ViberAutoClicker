using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Client.Database.Models;

namespace Client.ViewModels
{
    public class AccountViewModel : INotifyPropertyChanged
    {
        private ViberDb _db;
        private IEnumerable<Account> _accounts;

        public IEnumerable<Account> Accounts
        {
            get { return _accounts; }
            set
            {
                _accounts = value;
                OnPropertyChanged("Accounts");
            }
        }

        public AccountViewModel(ViberDb db)
        {
            _db = db;
            Accounts = db.LoadAccounts();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
