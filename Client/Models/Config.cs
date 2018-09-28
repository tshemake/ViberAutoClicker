using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class Config
    {
        private string _profilePath;
        public string ProfilePath
        {
            get
            {
                return _profilePath;
            }
            set
            {
                _profilePath = value;
                Properties.Settings.Default.ProfilesDirectoryPath = value;
                Properties.Settings.Default.Save();
            }
        }
        private string _viberClientPath;
        public string ViberClientPath
        {
            get
            {
                return _viberClientPath;
            }
            set
            {
                _viberClientPath = value;
                Properties.Settings.Default.ViberClientPath = value;
                Properties.Settings.Default.Save();
            }
        }
        private string _apiUrl;
        public string ApiUrl
        {
            get
            {
                return _apiUrl;
            }
            set
            {
                _apiUrl = value;
                Properties.Settings.Default.ApiUrl = value;
                Properties.Settings.Default.Save();
            }
        }

        private int _accountChangeAftere;
        public int AccountChangeAfter
        {
            get
            {
                return _accountChangeAftere;
            }
            set
            {
                _accountChangeAftere = value;
                Properties.Settings.Default.AccountChangeAfter = value;
                Properties.Settings.Default.Save();
            }
        }

        private int _maxCountMessage;
        public int MaxCountMessage
        {
            get
            {
                return _maxCountMessage;
            }
            set
            {
                _maxCountMessage = value;
                Properties.Settings.Default.MaxCountMessage = value;
                Properties.Settings.Default.Save();
            }
        }

        private int _pauseBetweenTasks;
        public int PauseBetweenTasks
        {
            get
            {
                return _pauseBetweenTasks;
            }
            set
            {
                _pauseBetweenTasks = value;
                Properties.Settings.Default.PauseBetweenTasks = value;
                Properties.Settings.Default.Save();
            }
        }

        public Config()
        {
            _profilePath = Properties.Settings.Default.ProfilesDirectoryPath;
            _viberClientPath = Properties.Settings.Default.ViberClientPath;
            _apiUrl = Properties.Settings.Default.ApiUrl;
            _accountChangeAftere = Properties.Settings.Default.AccountChangeAfter;
            _maxCountMessage = Properties.Settings.Default.MaxCountMessage;
            _pauseBetweenTasks = Properties.Settings.Default.PauseBetweenTasks;
        }
    }
}
