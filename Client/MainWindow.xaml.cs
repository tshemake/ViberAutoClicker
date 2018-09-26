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
using Client.ViewModels;

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

        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;
            DataContext = new Config();
            var db = new ViberDb(DefaultViberConfigDbInRoamingAppData());
            ViberAccounts.DataContext = new AccountViewModel(db);
        }

        private async void RegistrationNewAccount_Click(object sender, RoutedEventArgs e)
        {
            var config = (Config)DataContext;
            var client = Viber.Instance(config.ViberClientPath);
            client.Close();

            RegistrationNewAccount.IsEnabled = false;
            var db = new ViberDb(DefaultViberConfigDbInRoamingAppData());
            await db.OffAccountsAsync();

            client.Run();
            ViberAccounts.DataContext = await db.WaitNewAccountAsync();
            RegistrationNewAccount.IsEnabled = true;
        }

        private async void ChangeAccount_Click(object sender, EventArgs e)
        {
            try
            {
                var config = (Config) DataContext;
                var client = Viber.Instance(config.ViberClientPath);
                client.Close();
                ChangeAccount.IsEnabled = false;

                await SelectNextViberProfileAsync();
                client.Run();
            }
            finally
            {
                ChangeAccount.IsEnabled = true;
            }
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            var config = (Config)DataContext;

            if (string.IsNullOrEmpty(config.ProfilePath) || !Directory.Exists(config.ProfilePath))
            {
                System.Windows.MessageBox.Show("Выберите директорию с профилем");
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
            if (config.AccountChangeAfter <= 0)
            {
                System.Windows.MessageBox.Show("Укажите количество отправленных сообщений, после которых меняется аккаунт");
                return;
            }

            var client = Viber.Instance(config.ViberClientPath);
            client.StopIfRunning();
            ReplaceDefaultViberProfileInRoamingAppData(config.ProfilePath);
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            Start.IsEnabled = false;
            Stop.IsEnabled = true;
            await RunWork(client, token);
        }

        private async Task RunWork(Viber client, CancellationToken token)
        {
            var config = (Config)DataContext;

            API.BaseAddress = config.ApiUrl;
            client.Run();
            var count = 0;
            while (true)
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    var tasks = await API.GetTasksAsync();
                    var responeTask = new ResponeTask
                    {
                        Tasks = new List<Result>()
                    };

                    foreach (var domain in tasks.Domains)
                    {
                        try
                        {
                            foreach (var task in domain.Tasks)
                            {
                                if (CountViberProfile() > 1 && count >= config.AccountChangeAfter)
                                {
                                    client.Close();
                                    await SelectNextViberProfileAsync();
                                    client.Run();
                                    count = 1;
                                }

                                Guid statusId;
                                if (client.Send(task.Phone, domain.Message))
                                {
                                    count++;
                                }
                                switch (client.State)
                                {
                                    case ViberState.ClickMessageMenu:
                                        statusId = NotRegisteredId;
                                        break;
                                    case ViberState.SendMessage:
                                        statusId = Sended;
                                        break;
                                    case ViberState.Init:
                                    case ViberState.Start:
                                    case ViberState.Stop:
                                    case ViberState.Run:
                                    case ViberState.GoToMore:
                                    case ViberState.ClickPhoneNumberMenu:
                                    case ViberState.EnterPhoneNumber:
                                    case ViberState.IsEnableSendMessage:
                                    case ViberState.EnterMessage:
                                        statusId = Failure;
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
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

        private async Task SelectNextViberProfileAsync()
        { 
            var db = new ViberDb(DefaultViberConfigDbInRoamingAppData());
            var index = await db.GetNextAccountAsync();
        }

        private int CountViberProfile()
        {
            var db = new ViberDb(DefaultViberConfigDbInRoamingAppData());
            return db.CountAccount();
        }

        private static string DefaultViberProfileName()
        {
            return "ViberPC";
        }

        private static string DefaultViberProfileInRoamingAppData()
        {
            return System.IO.Path.Combine(HelpPath.RoamingAppData, DefaultViberProfileName());
        }

        private static string DefaultViberConfigDbInRoamingAppData()
        {
            return System.IO.Path.Combine(DefaultViberProfileInRoamingAppData(), "config.db");
        }

        private static void ChangeViberProfileInRoamingAppData(string profilePath)
        {
            var defaultPath = DefaultViberProfileInRoamingAppData();
            var defaultOldPath = defaultPath + "_old";
            if (Directory.Exists(defaultOldPath))
                    Directory.Delete(defaultOldPath, true);

            Directory.Move(profilePath, defaultPath);
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

        private static string ReplaceDefaultViberProfileInRoamingAppData(string profilePath)
        {
            var oldViberProfilePath = string.Empty;
            if (Directory.Exists(DefaultViberProfileInRoamingAppData()))
            {
                oldViberProfilePath = $"{DefaultViberProfileInRoamingAppData()}_{Guid.NewGuid()}";
                Directory.Move(DefaultViberProfileInRoamingAppData(), oldViberProfilePath);
            }

            HelpPath.DirectoryCopy(profilePath, System.IO.Path.Combine(HelpPath.RoamingAppData, DefaultViberProfileInRoamingAppData()));
            return oldViberProfilePath;
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
