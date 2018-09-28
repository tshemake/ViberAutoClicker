using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Client.Commands;
using Client.Database.Models;

namespace Client.ViewModels
{
    public class MainViewModel : UserControl
    {
        private static ViberDb _db;

        public Models.Config Config
        {
            get { return (Models.Config)GetValue(ConfigProperty); }
            set { SetValue(ConfigProperty, value); }
        }

        public IEnumerable<Account> Accounts
        {
            get { return (IEnumerable<Account>)GetValue(AccountsProperty); }
            set { SetValue(AccountsProperty, value); }
        }

        public MainViewModel(ViberDb db)
        {
            _db = db;
            LoadData();
            Config = new Models.Config();
        }

        public void LoadData()
        {
            Accounts = _db.LoadAccounts();
            foreach (var account in Accounts)
            {
                account.PropertyChanged += AccountsPropertyChanged;
            }
        }

        public static readonly DependencyProperty AccountsProperty = 
            DependencyProperty.Register("Accounts", typeof(IEnumerable<Account>), typeof(MainViewModel), new PropertyMetadata(new List<Account>()));
        public static readonly DependencyProperty ConfigProperty =
            DependencyProperty.Register("Config", typeof(Models.Config), typeof(MainViewModel), new PropertyMetadata(null));

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
