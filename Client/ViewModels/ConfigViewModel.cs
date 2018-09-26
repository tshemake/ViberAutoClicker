using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Client.Database.Models;

namespace Client.ViewModels
{
    class ConfigViewModel : DependencyObject
    {
        public Models.Config Config
        {
            get { return (Models.Config)GetValue(ConfigProperty); }
            set { SetValue(ConfigProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConfigProperty =
            DependencyProperty.Register("Config", typeof(Models.Config), typeof(ConfigViewModel), new PropertyMetadata(null));

        public ConfigViewModel()
        {
            Config = new Models.Config();
        }
    }
}
