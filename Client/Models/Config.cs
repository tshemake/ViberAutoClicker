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

        public Config()
        {
            _profilePath = Properties.Settings.Default.ProfilesDirectoryPath;
            _viberClientPath = Properties.Settings.Default.ViberClientPath;
            _apiUrl = Properties.Settings.Default.ApiUrl;
        }
    }
}
