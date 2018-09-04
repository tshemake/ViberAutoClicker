using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class ViberProfiles
    {
        private List<string> _profileNames = new List<string>();
        public ReadOnlyCollection<string> ProfileNames
        {
            get { return _profileNames.AsReadOnly(); }
        }
        private int _current;

        public int Count
        {
            get { return _profileNames.Count; }
        }

        public string CurrentProfile
        {
            get { return _current > -1 ? _profileNames[_current] : null; }
        }

        public ViberProfiles(List<string> profiles)
        {
            _profileNames = new List<string>(profiles);
            _current = -1;
        }

        public void Reload(List<string> profiles)
        {
            _profileNames = new List<string>(profiles);
            _current = -1;
        }

        public string GetNext()
        {
            if (Count == 0) return string.Empty;
            if (_current + 1 < _profileNames.Count)
            {
                _current++;
            }
            else
            {
                _current = 0;
            }
            return _profileNames[_current];
        }
    }
}
