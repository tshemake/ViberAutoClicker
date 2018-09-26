using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Client.Database.Models;

namespace Client.ViewModels
{
    public class AccountViewModel : UserControl
    {
        private static ViberDb _db;

        public IEnumerable<Account> Accounts
        {
            get { return (IEnumerable<Account>)GetValue(AccountsProperty); }
            set { SetValue(AccountsProperty, value); }
        }

        public AccountViewModel(ViberDb db)
        {
            _db = db;
            Accounts = db.LoadAccounts();
            foreach (var account in Accounts)
            {
                account.PropertyChanged += AccountsPropertyChanged;
            }
        }

        public static readonly DependencyProperty AccountsProperty = DependencyProperty.Register("Accounts", typeof(IEnumerable<Account>), typeof(AccountViewModel), new PropertyMetadata(new List<Account>()));

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public static async void AccountsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await _db.SaveAccountAsync(sender as Account);
        }
    }
}
