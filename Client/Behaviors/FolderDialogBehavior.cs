using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interactivity;

namespace Client.Behaviors
{
    class FolderDialogBehavior : Behavior<System.Windows.Controls.Button>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Click += OnClick;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Click -= OnClick;
            base.OnDetaching();
        }

        public static readonly DependencyProperty FolderNameProperty =
            DependencyProperty.RegisterAttached("FolderName", typeof(string), typeof(FolderDialogBehavior));

        public string FolderName
        {
            get { return (string)GetValue(FolderNameProperty); }
            set { SetValue(FolderNameProperty, value); }
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            var currentPath = FolderName as string;
            dialog.SelectedPath = currentPath;
            var result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                FolderName = dialog.SelectedPath;
            }
        }
    }
}
