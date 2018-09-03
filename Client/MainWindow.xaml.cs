using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Client.Native;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private ViberProfiles viberProfiles;

        public MainWindow()
        {
            InitializeComponent();
            ChangeProfilesDirectoryPath(Properties.Settings.Default.ProfilesDirectoryPath);
            if (string.IsNullOrEmpty(Properties.Settings.Default.ViberClientPath))
            {
                ChangeViberClientPath(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Viber\Viber.exe"));
            }
            else
            {
                ChangeViberClientPath(Properties.Settings.Default.ViberClientPath);
            }
            viberProfiles = new ViberProfiles(GetViberProfilesInRoamingAppData());
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ProfilePath.Text) || !Directory.Exists(ProfilePath.Text))
            {
                System.Windows.MessageBox.Show("Выберите директорию с профилями");
            }
            if (string.IsNullOrEmpty(ViberClientPath.Text) || !File.Exists(ViberClientPath.Text))
            {
                System.Windows.MessageBox.Show("Выберите Viber клиента");
            }

            var profileDirs = GetViberProfilePathsInProfilesDirectoryPath();
            CopyProfilesToRoamingAppData(profileDirs);
            viberProfiles.Reload(GetViberProfilesInRoamingAppData());
            ChangeViberProfileInRoamingAppDataToDefult(viberProfiles);
            StartViberClientIfNotRunning(ViberClientPath.Text);
        }

        private static string DefaultViberProfileInRoamingAppData()
        {
            return System.IO.Path.Combine(roamingAppData, "ViberPC");
        }

        private static void ChangeViberProfileInRoamingAppDataToDefult(ViberProfiles viberProfiles)
        {
            string defaultPath = DefaultViberProfileInRoamingAppData();
            if (viberProfiles.CurrentProfile == null)
            {
                if (Directory.Exists(defaultPath))
                    Directory.Delete(defaultPath, true);
            }
            else
            {
                Directory.Move(defaultPath, System.IO.Path.Combine(roamingAppData, viberProfiles.CurrentProfile));
            }
            string next = viberProfiles.GetNext();
            Directory.Move(System.IO.Path.Combine(roamingAppData, viberProfiles.CurrentProfile), defaultPath);
        }

        private static List<string> GetViberProfilePathsInProfilesDirectoryPath()
        {
            return Directory.EnumerateDirectories(Properties.Settings.Default.ProfilesDirectoryPath)
                        .Where(d => !string.IsNullOrEmpty(GetViberPcProfileNameFromPath(d))).ToList();
        }

        private static List<string> GetViberProfilesInRoamingAppData()
        {
            return Directory.EnumerateDirectories(roamingAppData)
                        .Select(d => GetViberPcProfileNameFromPath(d))
                        .Where(p => !string.IsNullOrEmpty(p)).ToList();
        }

        private static string GetViberPcProfileNameFromPath(string dir)
        {
            string dirName = System.IO.Path.GetFileName(dir);
            if (IsViberPcProfileDirName(dirName))
            {
                return dirName;
            }
            return string.Empty;
        }

        private static bool IsViberPcProfileDirName(string dir)
        {
            return Regex.Match(dir, @"^ViberPC_([0-9]{10})$").Success;
        }

        private static void CopyProfilesToRoamingAppData(List<string> newProfileDirs, bool replace = false)
        {
            var existsViberPcProfiles = GetViberProfilesInRoamingAppData();
            foreach (var newProfileDir in newProfileDirs)
            {
                string profileName = GetViberPcProfileNameFromPath(newProfileDir);
                bool existsProfile = existsViberPcProfiles.Contains(profileName);
                if (existsProfile && replace)
                {
                    Directory.Delete(System.IO.Path.Combine(roamingAppData, profileName), true);
                }
                if (!existsProfile)
                {
                    DirectoryCopy(newProfileDir, System.IO.Path.Combine(roamingAppData, profileName));
                }
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = System.IO.Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = System.IO.Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private static void StartViberClientIfNotRunning(string pathToViberClient, string processName = "Viber")
        {
            if (!IsViberClientRunning(processName))
            {
                StartViberClient(pathToViberClient);
            }
        }

        private static void StartViberClient(string pathToViberClient)
        {
            Process.Start(pathToViberClient);
            while (Win32Api.FindWindow("Qt5QWindowOwnDCIcon", IntPtr.Zero) == IntPtr.Zero)
            {
                Thread.Sleep(1000);
            }
            Thread.Sleep(3000);
        }

        private static bool IsViberClientRunning(string sProcessName)
        {
            return Process.GetProcessesByName(sProcessName).Length > 0;
        }

        private void ChangeProfilesDirectoryPath(string newPath)
        {
            if (string.IsNullOrEmpty(newPath)) return;

            if (newPath != ProfilePath.Text
                && Directory.Exists(newPath))
            {
                ProfilePath.Text = newPath;
                Properties.Settings.Default.ProfilesDirectoryPath = newPath;
                Properties.Settings.Default.Save();
            }
        }

        private void ChangeViberClientPath(string newPath)
        {
            if (string.IsNullOrEmpty(newPath)) return;

            if (newPath != ViberClientPath.Text
                && File.Exists(newPath))
            {
                ViberClientPath.Text = newPath;
                Properties.Settings.Default.ViberClientPath = newPath;
                Properties.Settings.Default.Save();
            }
        }

        private void SelectProfile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (!string.IsNullOrEmpty(Properties.Settings.Default.ProfilesDirectoryPath))
            {
                dialog.SelectedPath = Properties.Settings.Default.ProfilesDirectoryPath;
            }
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ChangeProfilesDirectoryPath(dialog.SelectedPath);
            }
        }

        private void SelectViberClient_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "Viber",
                DefaultExt = ".exe",
                Filter = "Viber client (.exe)|*.exe"
            };

            var result = dlg.ShowDialog();

            if (result == true)
            {
                ChangeViberClientPath(dlg.FileName);
            }
        }

        private static void StopViberClientIfRunning(string processName = "Viber")
        {
            if (IsViberClientRunning(processName))
            {
                Process[] processes = Process.GetProcessesByName(processName);
                foreach (var process in processes)
                {
                    foreach (var hWnd in GetRootWindowsOfProcess(process.Id))
                    {
                        CloseProcess(hWnd);
                    }
                    process.Kill();
                    process.WaitForExit(5000);
                }
            }
            if (IsViberClientRunning(processName))
            {
                StopViberClientIfRunning(processName);
            }
        }
        public IntPtr FindViberClient(string caption, bool wait = true)
        {
            int countAttempts = 3;
            IntPtr hWnd = Win32Api.FindWindowByCaption(IntPtr.Zero, caption);
            for (int i = 0; i <= countAttempts; i++)
            {
                if (hWnd != IntPtr.Zero) break;
                Thread.Sleep(100);
                hWnd = Win32Api.FindWindowByCaption(IntPtr.Zero, caption);
            }

            return hWnd;
        }

        private static bool CloseProcess(IntPtr hWnd)
        {
            IntPtr result = Win32Api.SendMessage(hWnd, Win32Api.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            return result == IntPtr.Zero;
        }

        private static List<IntPtr> GetRootWindowsOfProcess(int pid)
        {
            List<IntPtr> rootWindows = GetChildWindows(IntPtr.Zero);
            List<IntPtr> dsProcRootWindows = new List<IntPtr>();
            foreach (IntPtr hWnd in rootWindows)
            {
                Win32Api.GetWindowThreadProcessId(hWnd, out uint lpdwProcessId);
                if (lpdwProcessId == pid)
                    dsProcRootWindows.Add(hWnd);
            }
            return dsProcRootWindows;
        }

        private static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                Win32Api.Win32Callback childProc = new Win32Api.Win32Callback(EnumWindow);
                Win32Api.EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }

        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            List<IntPtr> list = gch.Target as List<IntPtr>;
            if (list == null)
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }
            list.Add(handle);
            //  You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }
    }
}
