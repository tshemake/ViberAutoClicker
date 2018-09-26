using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Client.Database.Models
{
    [Table("Accounts")]
    public class Account : INotifyPropertyChanged
    {
        private string _deviceKey;
        private string _token;
        private string _email;
        private string _nickName;
        private bool _isDefault = false;
        private bool _isAutoSignIn = false;
        private bool _isValid = false;
        private int _timeStamp = 0;
        [MaxLength(100)]
        [Column("ID")]
        public string Id { get; set; }
        [MaxLength(100)]
        public string DeviceKey
        {
            get { return _deviceKey; }
            set
            {
                _deviceKey = value;
                OnPropertyChanged("DeviceKey");
            }
        }
        [MaxLength(100)]
        public string Token
        {
            get { return _token; }
            set
            {
                _token = value;
                OnPropertyChanged("Token");
            }
        }
        [MaxLength(1000)]
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged("Email");
            }
        }
        [MaxLength(100)]
        public string NickName
        {
            get { return _nickName; }
            set
            {
                _nickName = value;
                OnPropertyChanged("NickName");
            }
        }
        public bool IsDefault
        {
            get { return _isDefault; }
            set
            {
                _isDefault = value;
                OnPropertyChanged("IsDefault");
            }
        }
        public bool IsAutoSignIn
        {
            get { return _isAutoSignIn; }
            set
            {
                _isAutoSignIn = value;
                OnPropertyChanged("IsAutoSignIn");
            }
        }
        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                _isValid = value;
                OnPropertyChanged("IsValid");
            }
        }
        public int TimeStamp
        {
            get { return _timeStamp; }
            set
            {
                _timeStamp = value;
                OnPropertyChanged("TimeStamp");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}