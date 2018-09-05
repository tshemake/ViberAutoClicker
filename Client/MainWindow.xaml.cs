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
        private static Guid NotRegisteredId = Guid.Parse("6B75DE8A-480E-4396-8368-E4ED2E851E9D");

        private ViberProfiles _viberProfiles;
        private Viber _client;

        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;
            _viberProfiles = new ViberProfiles(GetViberProfilesInRoamingAppData());
            DataContext = new Config();
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            Config config = (Config)DataContext;

            if (string.IsNullOrEmpty(config.ProfilePath) || !Directory.Exists(config.ProfilePath))
            {
                System.Windows.MessageBox.Show("Выберите директорию с профилями");
                return;
            }
            if (string.IsNullOrEmpty(config.ViberClientPath) || !File.Exists(config.ViberClientPath))
            {
                System.Windows.MessageBox.Show("Выберите Viber клиента");
                return;
            }
            if (string.IsNullOrEmpty(config.ApiUrl))
            {
                System.Windows.MessageBox.Show("Укажите URL сервера");
                return;
            }

            _client = new Viber(config.ViberClientPath);
            _client.Close();
            var profileDirs = GetViberProfilePathsInProfilesDirectoryPath();
            CopyProfilesToRoamingAppData(profileDirs);
            _viberProfiles.Reload(GetViberProfilesInRoamingAppData());
            ChangeViberProfileInRoamingAppData(_viberProfiles);
            _cts = new CancellationTokenSource();
            CancellationToken _token = _cts.Token;
            Start.IsEnabled = false;
            Stop.IsEnabled = true;
            await RunWork(_token);
        }

        private async Task RunWork(CancellationToken token, int maxCountMessage = 50)
        {
            Config config = (Config)DataContext;

            API.BaseAddress = config.ApiUrl;
            _client.Run();
            int count = 0;
            while (true)
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    InfoTask tasks = await API.GetTasksAsync();
                    ResponeTask responeTask = new ResponeTask
                    {
                        Tasks = new List<Result>()
                    };

                    foreach (var domain in tasks.Domains)
                    {
                        try
                        {
                            foreach (var task in domain.Tasks)
                            {
                                if (count >= maxCountMessage && _viberProfiles.Count > 1)
                                {
                                    _client.Close();
                                    ChangeViberProfileInRoamingAppData(_viberProfiles);
                                    _client.Run();
                                    count = 1;
                                }

                                Guid statusId = Failure;
                                if (_client.Send(task.Phone, domain.Message))
                                {
                                    count++;
                                }
                                switch (_client.State)
                                {
                                    case 4:
                                        statusId = NotRegisteredId;
                                        break;
                                    case 7:
                                        statusId = Sended;
                                        break;
                                    default:
                                        statusId = Failure;
                                        break;
                                }
                                responeTask.Tasks.Add(new Result
                                {
                                    Id = task.Id,
                                    StatusId = statusId
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                        finally
                        {
                            if (responeTask.Tasks.Any())
                                await API.UpdateTasksAsync(responeTask);
                        }
                    }
                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            Start.IsEnabled = true;
        }

        private static string DefaultViberProfileInRoamingAppData()
        {
            return System.IO.Path.Combine(HelpPath.RoamingAppData, "ViberPC");
        }

        private static void ChangeViberProfileInRoamingAppData(ViberProfiles viberProfiles)
        {
            string defaultPath = DefaultViberProfileInRoamingAppData();
            if (viberProfiles.CurrentProfile == null)
            {
                if (Directory.Exists(defaultPath))
                    Directory.Delete(defaultPath, true);
            }
            else
            {
                Directory.Move(defaultPath, System.IO.Path.Combine(HelpPath.RoamingAppData, viberProfiles.CurrentProfile));
            }
            string next = viberProfiles.GetNext();
            Directory.Move(System.IO.Path.Combine(HelpPath.RoamingAppData, viberProfiles.CurrentProfile), defaultPath);
        }

        private static List<string> GetViberProfilePathsInProfilesDirectoryPath()
        {
            return Directory.EnumerateDirectories(Properties.Settings.Default.ProfilesDirectoryPath)
                        .Where(d => !string.IsNullOrEmpty(GetViberPcProfileNameFromPath(d))).ToList();
        }

        private static List<string> GetViberProfilesInRoamingAppData()
        {
            return Directory.EnumerateDirectories(HelpPath.RoamingAppData)
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
                    Directory.Delete(System.IO.Path.Combine(HelpPath.RoamingAppData, profileName), true);
                }
                if (!existsProfile)
                {
                    HelpPath.DirectoryCopy(newProfileDir, System.IO.Path.Combine(HelpPath.RoamingAppData, profileName));
                }
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
            Stop.IsEnabled = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            DataContext = null;
        }
    }
}
