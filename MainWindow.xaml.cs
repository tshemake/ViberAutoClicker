using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;

namespace ViberAutoClicker
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Timers.Timer _timer;
        private Win32API.RECT? _viberClient;

        public MainWindow()
        {
            InitializeComponent();
            _timer = new System.Timers.Timer(100);
            _timer.Elapsed += (sender, e) => HandleTimer();
            _timer.Start();
        }

        private void SearchViberClients_Click(object sender, RoutedEventArgs e)
        {
            ReloadViberClientList();
        }

        private void GetWindowSizeViberClient_Click(object sender, RoutedEventArgs e)
        {
            if (ViberClients.SelectedIndex == -1)
            {
                MessageBox.Show("Client not selected");
                return;
            }

            IntPtr windowPtr = FindWindow(ViberClients.SelectedItem.ToString());
            if (windowPtr == IntPtr.Zero)
            {
                Debug.WriteLine("Client not found");
                return;
            }
            if (UpdatePositionViberClient())
            {
                BringWindowToTop(new WindowInteropHelper(this).Handle);
                MessageBox.Show($"[{_viberClient.Value.Left},{_viberClient.Value.Top}] - [{_viberClient.Value.Right},{_viberClient.Value.Bottom}]");
            }
        }

        public IntPtr FindWindow(string windowName, bool wait = true)
        {
            int countAttempts = 3;
            IntPtr hWnd = Win32API.FindWindowByCaption(IntPtr.Zero, windowName);
            for (int i = 0; i <= countAttempts; i++)
            {
                if (hWnd != IntPtr.Zero) break;
                System.Threading.Thread.Sleep(100);
                hWnd = Win32API.FindWindowByCaption(IntPtr.Zero, windowName);
            }

            return hWnd;
        }

        private void SetWindowSize(IntPtr hWnd)
        {
            int X = 709, Y = 49, cx = 1091, cy = 793;
            Win32API.SetWindowPos(hWnd, IntPtr.Zero, X, Y, cx, cy, Win32API.SetWindowPosFlags.SWP_SHOWWINDOW);
        }

        private bool BringWindowToTop(IntPtr hWnd)
        {
            return Win32API.SetForegroundWindow(hWnd);
        }

        private bool UpdatePositionViberClient()
        {
            Win32API.RECT rct;
            Rectangle myRect = new Rectangle();

            IntPtr hWnd = FindWindow(ViberClients.SelectedItem.ToString());
            if (hWnd == IntPtr.Zero)
            {
                Debug.WriteLine("Client not found");
                return false;
            }
            SetWindowSize(hWnd);
            BringWindowToTop(hWnd);

            if (!Win32API.GetWindowRect(new HandleRef(this, hWnd), out rct))
            {
                return false;
            }
            _viberClient = rct;
            return true;
        }

        public static Win32API.POINT GetCursorPos()
        {
            var gotPoint = Win32API.GetCursorPos(out Win32API.POINT currentMousePoint);
            return currentMousePoint;
        }

        private void ReloadViberClientList()
        {
            ViberClients.Items.Clear();
            List<string> clients = FindViberClients(IntPtr.Zero, "Viber");
            foreach (var title in clients)
            {
                ViberClients.Items.Add(title);
            }
            if (clients.Count > 0)
            {
                ViberClients.SelectedIndex = 0;
            }
        }

        private void ViberClients_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (ViberClients.SelectedIndex == -1) return;

            IntPtr windowPtr = FindWindow(ViberClients.SelectedItem.ToString());
            if (windowPtr == IntPtr.Zero)
            {
                Debug.WriteLine("Client not found");
                return;
            }
            UpdatePositionViberClient();
        }

        private void CloseViberClient_Click(object sender, RoutedEventArgs e)
        {
            if (ViberClients.SelectedIndex == -1) return;

            IntPtr windowPtr = FindWindow(ViberClients.SelectedItem.ToString());
            if (windowPtr == IntPtr.Zero)
            {
                Debug.WriteLine("Client not found");
                return;
            }

            IntPtr result = Win32API.SendMessage(windowPtr, Win32API.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            if (result == IntPtr.Zero)
            {
                ReloadViberClientList();
            }
        }

        public void PressMore_Click(object sender, RoutedEventArgs e)
        {
            UpdatePositionViberClient();
            Click(257, 104);
        }

        public void PressSendMessage_Click(object sender, RoutedEventArgs e)
        {
            UpdatePositionViberClient();
            Click(214, 520);
            var color = GetPixelColor(700, 760);
            if (color.HasValue)
            {
                Debug.WriteLine("Client don't have account in viber");
                return;
            }
            System.Windows.Clipboard.SetDataObject(MessageContent.Text, true);
            Click(700, 760);
            SendKeys.SendWait("^{v}");
            Click(1000, 760);
        }

        public void PressEnterPhoneNumber_Click(object sender, RoutedEventArgs e)
        {
            var chars = PhoneNumber.Text.Where(c => char.IsDigit(c));
            if (chars.Count() != 10) return;

            UpdatePositionViberClient();
            Click(171, 407);
            System.Threading.Thread.Sleep(100);
            Click(163, 163);
            System.Threading.Thread.Sleep(1000);
            IntPtr windowPtr = FindWindow(ViberClients.SelectedItem.ToString());
            if (windowPtr == IntPtr.Zero)
            {
                Debug.WriteLine("Client not found");
                return;
            }
            foreach (var ch in chars)
            {
                Win32API.PostMessage(windowPtr, Win32API.WM_KEYDOWN, (IntPtr)ch, IntPtr.Zero);
            }
        }

        public static List<string> FindViberClients(IntPtr parent, string windowCaption, string windowClass = "Qt5QWindowOwnDCIcon")
        {
            List<string> clients = new List<string>();
            Win32API.EnumWindows((hWnd, lParam) =>
            {
                if (!Win32API.IsWindowVisible(hWnd)) { return true; }
                int size = Win32API.GetWindowTextLength(hWnd);
                if (size <= 0) { return true; }
                StringBuilder sb = new StringBuilder(size + 1);
                Win32API.GetWindowText(hWnd, sb, size + 1);
                if (!sb.ToString().Contains(windowCaption)) { return true; }
                StringBuilder className = new StringBuilder(255);
                Win32API.GetClassName(hWnd, className, className.Capacity);
                if (className.ToString() == "Qt5QWindowOwnDCIcon") { clients.Add(sb.ToString()); }
                return true;
            }, IntPtr.Zero);
            return clients;
        }

        private System.Drawing.Color? GetPixelColor(int x, int y)
        {
            if (ViberClients.SelectedIndex == -1) return null;

            IntPtr windowPtr = FindWindow(ViberClients.SelectedItem.ToString());
            if (windowPtr == IntPtr.Zero)
            {
                return null;
            }
            IntPtr hdc = Win32API.GetDC(IntPtr.Zero);
            uint pixel = Win32API.GetPixel(hdc, x + _viberClient.Value.Left, y + _viberClient.Value.Top);
            if (pixel == 0xFFFFFF) return null;
            Win32API.ReleaseDC(IntPtr.Zero, hdc);
            System.Drawing.Color? color = System.Drawing.Color.FromArgb((int)(pixel & 0x000000FF),
                            (int)(pixel & 0x0000FF00) >> 8,
                            (int)(pixel & 0x00FF0000) >> 16);
            return color;
        }

        private bool Click(int x, int y)
        {
            Win32API.POINT sourcePos = GetCursorPos();
            int destPosX = x + _viberClient.Value.Left;
            int destPosY = y + _viberClient.Value.Top;
            Win32API.SetCursorPos(destPosX, destPosY);
            Win32API.mouse_event(Win32API.MOUSEEVENTF_LEFTDOWN, destPosX, destPosX, 0, 0);
            Win32API.mouse_event(Win32API.MOUSEEVENTF_LEFTUP, destPosY, destPosY, 0, 0);
            Win32API.SetCursorPos(sourcePos.X, sourcePos.Y);
            return true;
        }

        private void HandleTimer()
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    string msg = string.Empty;
                    Win32API.POINT pos = GetCursorPos();
                    if (_viberClient.HasValue
                        && _viberClient.Value.Top < pos.Y && _viberClient.Value.Bottom > pos.Y
                        && _viberClient.Value.Left < pos.X && _viberClient.Value.Right > pos.X)
                    {
                        msg += $"Pos: {pos.X - _viberClient.Value.Left} - {pos.Y - _viberClient.Value.Top}";
                        System.Drawing.Color? color = GetPixelColor(pos.X - _viberClient.Value.Left, pos.Y - _viberClient.Value.Top);
                        if (color.HasValue)
                        {
                            msg += ", Color: " + color.Value.Name;
                        }
                    }
                    else
                    {
                        msg += $"Pos: {pos.X} - {pos.Y}";
                    }
                    CursorPosition.Text = msg;
                }));
            }
            catch { }
        }
    }
}
