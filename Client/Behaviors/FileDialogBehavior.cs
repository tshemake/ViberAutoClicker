using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interactivity;
using Microsoft.Win32;

namespace Client.Behaviors
{
    public class FileDialogBehavior : Behavior<System.Windows.Controls.Button>
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

        public string FileName
        {
            get { return (string)GetValue(SelectedFileNameProperty); }
            set { SetValue(SelectedFileNameProperty, value); }
        }

        public static readonly DependencyProperty SelectedFileNameProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(FileDialogBehavior), new UIPropertyMetadata(""));

        public List<string> FileNames
        {
            get { return (List<string>)GetValue(SelectedFileNamesProperty); }
            set { SetValue(SelectedFileNamesProperty, value); }
        }

        public static readonly DependencyProperty SelectedFileNamesProperty =
            DependencyProperty.Register("FileNames", typeof(List<string>), typeof(FileDialogBehavior), new UIPropertyMetadata(new List<string>()));

        public string FilterString
        {
            get { return (string)GetValue(FilterStringProperty); }
            set { SetValue(FilterStringProperty, value); }
        }
        public static readonly DependencyProperty FilterStringProperty =
            DependencyProperty.Register("FilterString", typeof(string), typeof(FileDialogBehavior), new UIPropertyMetadata(""));

        public bool IsFileNameValid
        {
            get { return (bool)GetValue(IsFileNameValidProperty); }
            set { SetValue(IsFileNameValidProperty, value); }
        }
        public static readonly DependencyProperty IsFileNameValidProperty =
            DependencyProperty.Register("IsFileNameValid", typeof(bool), typeof(FileDialogBehavior), new UIPropertyMetadata(false));

        public bool IsMultiselect
        {
            get { return (bool)GetValue(IsMultiselectValidProperty); }
            set { SetValue(IsMultiselectValidProperty, value); }
        }
        public static readonly DependencyProperty IsMultiselectValidProperty =
            DependencyProperty.Register("IsMultiselect", typeof(bool), typeof(FileDialogBehavior), new UIPropertyMetadata(false));

        public FileDialogBehavior()
        {
        }

        private void OnClick(object sender, EventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog())
            {
                ofd.AddExtension = true;
                ofd.Filter = FilterString;
                ofd.Multiselect = IsMultiselect;
                if (ofd.ShowDialog() != DialogResult.OK)
                {
                    IsFileNameValid = false;
                }
                else
                {
                    FileName = ofd.FileName;
                    IsFileNameValid = true;
                    foreach (string fileName in ofd.FileNames)
                    {
                        FileNames.Add(fileName);
                    }
                }
            }

        }
    }
}
