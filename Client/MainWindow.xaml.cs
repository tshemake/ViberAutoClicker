using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Client.Helpers;
using Client.Models;
using Client.Native;
using Client.ViewModels;
using IOException = System.IO.IOException;

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

        private readonly BackgroundWorker _worker = new BackgroundWorker();

        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;
            _worker.DoWork += Worker_DoWork;
            _worker.RunWorkerAsync();
            var db = new ViberDb(DefaultViberConfigDbInRoamingAppData());
            DataContext = new MainViewModel(db);
        }

        private async void RegistrationNewAccount_Click(object sender, RoutedEventArgs e)
        {
            var viewMode = (MainViewModel)DataContext;

            var client = Viber.Instance(viewMode.Config.ViberClientPath);
            client.Close();

            SetDisabled(RegistrationNewAccount);
            var db = new ViberDb(DefaultViberConfigDbInRoamingAppData());
            await db.OffAccountsAsync();

            client.Run();
            ViberAccounts.DataContext = await db.WaitNewAccountAsync();
            SetEnabled(RegistrationNewAccount);
        }

        public async void ChangeViberProfile_Click(object sender, RoutedEventArgs e)
        {
            var viewMode = (MainViewModel)DataContext;

            var client = Viber.Instance(viewMode.Config.ViberClientPath);
            client.Close();
            SetDisabled(Start, ChangeViberProfile);
            await ReplaceDefaultViberProfileInRoamingAppData(viewMode.Config.ProfilePath);
        }

        private Task ReplaceDefaultViberProfileInRoamingAppData(string profilePath)
        {
            var viewMode = (MainViewModel)DataContext;

            var t = new Task(() =>
            {
                var oldViberProfilePath = string.Empty;
                if (Directory.Exists(DefaultViberProfileInRoamingAppData()))
                {
                    oldViberProfilePath = $"{DefaultViberProfileInRoamingAppData()}_{Guid.NewGuid()}";
                    while (!Directory.Exists(oldViberProfilePath))
                    {
                        try
                        {
                            Directory.Move(DefaultViberProfileInRoamingAppData(), oldViberProfilePath);
                        }
                        catch (IOException)
                        {
                            Thread.Sleep(500);
                        }
                    }
                }

                HelpPath.DirectoryCopy(profilePath, System.IO.Path.Combine(HelpPath.RoamingAppData, DefaultViberProfileInRoamingAppData()));
                DispatcherHelper.CheckBeginInvokeOnUI(
                    (Action)(() =>
                        {
                            SetEnabled(Start, ChangeViberProfile);
                            viewMode.LoadData();
                            ViberAccounts.UpdateLayout();
                        }
                    ));
            });
            t.Start();
            return t;
        }

        private static void SetEnabled(params UIElement[] elements)
        {
            SetEnabledProperty(true, elements);
        }

        private static void SetDisabled(params UIElement[] elements)
        {
            SetEnabledProperty(false, elements);
        }

        private static void SetEnabledProperty(bool isEnabled, params UIElement[] elements)
        {
            foreach (var element in elements)
            {
                element.IsEnabled = isEnabled;
            }
        }

        private async void ChangeAccount_Click(object sender, EventArgs e)
        {
            try
            {
                var viewMode = (MainViewModel)DataContext;

                var client = Viber.Instance(viewMode.Config.ViberClientPath);
                client.Close();
                SetDisabled(ChangeAccount);

                await SelectNextViberProfileAsync();
                client.Run();
            }
            finally
            {
                SetEnabled(ChangeAccount);
            }
        }

        private List<KeyValuePair<bool, string>> CheckConfig()
        {
            var viewMode = (MainViewModel)DataContext;

            var errors = new List<KeyValuePair<bool, string>>();

            if (string.IsNullOrEmpty(viewMode.Config.ProfilePath) || !Directory.Exists(viewMode.Config.ProfilePath))
            {
                errors.Add(new KeyValuePair<bool, string>(false, "Выберите директорию с профилем"));
            }
            if (string.IsNullOrEmpty(viewMode.Config.ViberClientPath) || !File.Exists(viewMode.Config.ViberClientPath))
            {
                errors.Add(new KeyValuePair<bool, string>(false, "Выберите Viber клиента"));
            }
            if (string.IsNullOrEmpty(viewMode.Config.ApiUrl))
            {
                errors.Add(new KeyValuePair<bool, string>(false, "Укажите URL сервера"));
            }
            if (viewMode.Config.AccountChangeAfter <= 0)
            {
                errors.Add(new KeyValuePair<bool, string>(false, "Укажите количество отправленных сообщений, после которых меняется аккаунт"));
            }
            if (viewMode.Config.MaxCountMessage <= 0)
            {
                errors.Add(new KeyValuePair<bool, string>(false, "Укажите размер пакета получаемых сообщений"));
            }
            if (viewMode.Config.PauseBetweenTasks <= 0)
            {
                errors.Add(new KeyValuePair<bool, string>(false, "Укажите паузу между получением пакета сообщений"));
            }

            return errors;
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            var viewMode = (MainViewModel)DataContext;

            var errors = CheckConfig();
            if (errors.Any())
            {
                System.Windows.MessageBox.Show(string.Join("\n", errors.Select(err => err.Value)));
                return;
            }

            var client = Viber.Instance(viewMode.Config.ViberClientPath);
            client.StopIfRunning();

            SetDisabled(Start);
            SetEnabled(Stop);

            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            await RunWork(client, viewMode.Config, token);
        }

        private Task RunWork(Viber client, Config config, CancellationToken token)
        {
            API.BaseAddress = config.ApiUrl;
            client.Run();

            var t = new Task(async () =>
            {
                try
                {
                    var count = 0;
                    while (true)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        var tasks = await API.GetTasksAsync(config.MaxCountMessage);
                        var responseTask = new ResponseTask
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
                                        case ViberState.Run:
                                            return;
                                        default:
                                            statusId = Failure;
                                            break;
                                    }

                                    responseTask.Tasks.Add(new Result
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
                                if (responseTask.Tasks.Any())
                                    await API.UpdateTasksAsync(responseTask);
                            }
                        }

                        Thread.Sleep(config.PauseBetweenTasks);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                finally
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(
                        (Action) (() =>
                            {
                                SetDisabled(Stop);
                                SetEnabled(Start);
                            }
                        ));
                }
            });
            t.Start();

            return t;
        }

        private async Task SelectNextViberProfileAsync()
        {
            var db = new ViberDb(DefaultViberConfigDbInRoamingAppData());
            var index = await db.GetNextActiveAccountAsync();
        }

        private int CountViberProfile()
        {
            var db = new ViberDb(DefaultViberConfigDbInRoamingAppData());
            return db.CountActiveAccount();
        }

        private static string DefaultViberProfileInRoamingAppData()
        {
            return System.IO.Path.Combine(HelpPath.RoamingAppData, DefaultViberProfileName());
        }

        private static string DefaultViberProfileName()
        {
            return "ViberPC";
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

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(
                    (Action)(() =>
                    {
                        var position = Native.Win32Api.GetCursorPosition();
                        var color = Native.Win32Api.GetPixelColor(position);
                        Status.Text = $"[{position.X} - {position.Y}] => {color}({color.ToArgb()})";
                    }
                    ));
                Thread.Sleep(100);
            }
        }
    }
}
