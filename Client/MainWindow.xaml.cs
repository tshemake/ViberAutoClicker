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
using Client.Models;
using Client.Native;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Guid Delivered = Guid.Parse("82421000-ea57-44e0-8a2f-d2ec159f8fde");
        private static Guid Failure = Guid.Parse("09bdbdee-46ea-451a-8049-4d1390be8b25");
        private static Guid Sended = Guid.Parse("7e2f259b-8c03-4ba9-8363-5324466c475e");

        private static string _roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private ViberProfiles _viberProfiles;
        private Viber _client;
        private Config _config = new Config();

        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();
            _viberProfiles = new ViberProfiles(GetViberProfilesInRoamingAppData());
            DataContext = _config;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_config.ProfilePath) || !Directory.Exists(_config.ProfilePath))
            {
                System.Windows.MessageBox.Show("Выберите директорию с профилями");
            }
            if (string.IsNullOrEmpty(_config.ViberClientPath) || !File.Exists(_config.ViberClientPath))
            {
                System.Windows.MessageBox.Show("Выберите Viber клиента");
            }
            if (string.IsNullOrEmpty(_config.ApiUrl) || !File.Exists(_config.ApiUrl))
            {
                System.Windows.MessageBox.Show("Укажите URL сервера");
            }

            _client = new Viber(_config.ViberClientPath);
            _client.Stop();
            var profileDirs = GetViberProfilePathsInProfilesDirectoryPath();
            CopyProfilesToRoamingAppData(profileDirs);
            _viberProfiles.Reload(GetViberProfilesInRoamingAppData());
            ChangeViberProfileInRoamingAppDataToDefult(_viberProfiles);
            Start.IsEnabled = false;
            Stop.IsEnabled = true;
            _cts = new CancellationTokenSource();
            CancellationToken _token = _cts.Token;
            Task.Run(() => RunWork(_token));
        }

        private async void RunWork(CancellationToken token, int maxQueue = 10)
        {
            API.BaseAddress = _config.ApiUrl;
            _client.Run();
            int count = 0;
            while (true)
            {
                try
                {
                    InfoTask infoTask = await API.GetTasksAsync();

                    ResponeTask responeTask = new ResponeTask
                    {
                        Tasks = new List<Result>()
                    };

                    foreach (var domain in infoTask.Domains)
                    {
                        try
                        {
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }
                            foreach (var task in domain.Tasks)
                            {
                                count++;
                                if (count >= maxQueue)
                                {
                                    _client.Stop();
                                    _client.Run();
                                    count = 1;
                                }
                                if (_client.Send(task.Phone, domain.Message))
                                {
                                    responeTask.Tasks.Add(new Result
                                    {
                                        Id = task.Id,
                                        StatusId = Sended
                                    });
                                }
                                else
                                {
                                    responeTask.Tasks.Add(new Result
                                    {
                                        Id = task.Id,
                                        StatusId = Failure
                                    });
                                }
                            }

                            await API.UpdateTasksAsync(responeTask);
                        }
                        finally
                        {
                            _client.Stop();
                        }
                    }
                }
                catch
                {
                    System.Windows.MessageBox.Show("Проверьте доступность сервера");
                    _cts.Cancel();
                    break;
                }
                finally
                {
                    _cts.Dispose();
                }
            }
        }

        private static string DefaultViberProfileInRoamingAppData()
        {
            return System.IO.Path.Combine(_roamingAppData, "ViberPC");
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
                Directory.Move(defaultPath, System.IO.Path.Combine(_roamingAppData, viberProfiles.CurrentProfile));
            }
            string next = viberProfiles.GetNext();
            Directory.Move(System.IO.Path.Combine(_roamingAppData, viberProfiles.CurrentProfile), defaultPath);
        }

        private static List<string> GetViberProfilePathsInProfilesDirectoryPath()
        {
            return Directory.EnumerateDirectories(Properties.Settings.Default.ProfilesDirectoryPath)
                        .Where(d => !string.IsNullOrEmpty(GetViberPcProfileNameFromPath(d))).ToList();
        }

        private static List<string> GetViberProfilesInRoamingAppData()
        {
            return Directory.EnumerateDirectories(_roamingAppData)
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
                    Directory.Delete(System.IO.Path.Combine(_roamingAppData, profileName), true);
                }
                if (!existsProfile)
                {
                    DirectoryCopy(newProfileDir, System.IO.Path.Combine(_roamingAppData, profileName));
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
                _config.ProfilePath = dialog.SelectedPath;
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
                _config.ViberClientPath = dlg.FileName;
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _cts.Cancel();
            Start.IsEnabled = true;
            Stop.IsEnabled = false;
        }
    }
}
