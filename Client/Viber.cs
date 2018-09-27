using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Client.Native;

namespace Client
{
    public class Viber : IDisposable
    {
        private static volatile Viber _instance;
        private static readonly object SyncRoot = new object();

        private const string ProcessName = "Viber";

        private readonly Mouse _virtualMouse;

        public ViberState State { get; private set; }

        public string ExePath { get; private set; }

        public static Viber Instance(string path)
        {
            if (_instance == null)
            {
                lock (SyncRoot)
                {
                    if (_instance == null)
                        _instance = new Viber(path);
                }
            }
            else
            {
                lock (SyncRoot)
                {
                    if (string.IsNullOrEmpty(path) || path == _instance.ExePath) return _instance;
                    _instance.ExePath = path;
                    if (IsRunning()) _instance.Restart();
                }
            }
            return _instance;
        }

        private Viber(string path)
        {
            State = ViberState.Init;
            ExePath = path;
            _virtualMouse = new Mouse(0, 0);
        }

        public bool Run()
        {
            StopIfRunning();
            var hWnd = Start();
            if (hWnd == IntPtr.Zero)
            {
                return false;
            }

            SetWindowSize(hWnd);
            State = ViberState.Run;
            return true;
        }

        public bool Send(string phoneNumber, string message)
        {
            if (IsMessageSyncShow()) CloseMessageSync();

            if (!GoToMore()) return false;
            State = ViberState.GoToMore;
            if (!ClickPhoneNumberMenu()) return false;
            State = ViberState.ClickPhoneNumberMenu;
            if (!EnterPhoneNumber(phoneNumber)) return false;
            State = ViberState.EnterPhoneNumber;
            if (!ClickMessageMenu()) return false;
            State = ViberState.ClickMessageMenu;
            if (!IsEnableSendMessage()) return false;
            State = ViberState.IsEnableSendMessage;
            if (!EnterMessage(message)) return false;
            State = ViberState.EnterMessage;
            if (!SendMessage()) return false;
            State = ViberState.SendMessage;
            return true;
        }

        private bool Do(Action action, int countAttempts, params Position[] positions)
        {
            bool successful = false;
            if (IsOpen())
            {
                for (int i = 0; i <= countAttempts && !successful; i++)
                {
                    if (!SetForegroundWindow()) continue;
                    Thread.Sleep(100);
                    action();
                    Thread.Sleep(600);
                    bool isMatch = true;
                    foreach (var position in positions)
                    {
                        Color? color = GetPixelColor(_positionMap[position]);
                        if (!color.HasValue || color.Value.ToArgb() != _positionMap[position].Argb)
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    if (isMatch) successful = true;
                }
            }

            return successful;
        }

        public bool GoToMore(int countAttempts = 5)
        {
            return Do(delegate ()
            {
                _virtualMouse.MoveTo(_positionMap[Position.More]).LeftClick();
            }, countAttempts, Position.More);
        }

        public bool ClickPhoneNumberMenu(int countAttempts = 5)
        {
            return Do(delegate ()
            {
                _virtualMouse.MoveTo(_positionMap[Position.PhoneNumberMenu]).LeftClick();
            }, countAttempts, Position.More);
        }

        public bool EnterPhoneNumber(string phoneNumber, int countAttempts = 5)
        {
            return Do(delegate ()
            {
                System.Windows.Clipboard.SetDataObject(phoneNumber, true);
                Thread.Sleep(100);
                _virtualMouse.MoveTo(_positionMap[Position.PhoneNumberEdit]).LeftClick();
                SendSelectAll();
                SendPaste();
            }, countAttempts, Position.More);
        }

        public bool ClickMessageMenu(int countAttempts = 5)
        {
            return Do(delegate ()
            {
                _virtualMouse.MoveTo(_positionMap[Position.MessageButton]).LeftClick();
            }, countAttempts, Position.DialogChat);
        }

        private bool IsEnableSendMessage(int countAttempts = 5)
        {
            SetForegroundWindow();
            Thread.Sleep(600);
            Color? color = GetPixelColor(_positionMap[Position.MessageEditBlock]);
            if (!color.HasValue)
            {
                return false;
            }
            return true;
        }

        public bool EnterMessage(string message, int countAttempts = 1)
        {
            return Do(delegate ()
            {
                _virtualMouse.MoveTo(_positionMap[Position.MessageEdit]).LeftClick();
                System.Windows.Clipboard.SetText(message);
                SendSelectAll();
                SendPaste();
            }, countAttempts, Position.MessageEditBlock);
        }

        public bool SendMessage(int countAttempts = 5)
        {
            return Do(delegate ()
            {
                SendEnter();
            }, countAttempts, Position.MessageEditBlock);
        }

        public void Close()
        {
            StopIfRunning();
        }

        private static void SendSelectAll()
        {
            Win32Api.PressCtrlKey('A');
        }

        private static void SendPaste()
        {
            Win32Api.PressCtrlKey('V');
        }

        private static void SendEnter()
        {
            Win32Api.keybd_event(Win32Api.Keys.VK_ENTER, 0, Win32Api.Keys.KEYEVENTF_KEYDOWN, 0);
            Win32Api.keybd_event(Win32Api.Keys.VK_ENTER, 0, Win32Api.Keys.KEYEVENTF_KEYUP, 0);
        }

        private static void SetWindowSize(IntPtr hWnd)
        {
            int x = 0, y = 0, cx = 1091, cy = 618;
            Win32Api.SetWindowPos(hWnd, IntPtr.Zero, x, y, cx, cy, Win32Api.SetWindowPosFlags.SWP_SHOWWINDOW);
        }

        private bool SetForegroundWindow()
        {
            IntPtr hWnd = FindWindow();
            if (hWnd == IntPtr.Zero)
                return false;

            return Win32Api.SetForegroundWindow(hWnd);
        }

        public IntPtr Start()
        {
            Process.Start(ExePath);
            while (!IsOpen())
            {
                Thread.Sleep(500);
            }

            Thread.Sleep(3000);

            var hWnd = FindWindow();

            var lpString = new StringBuilder(Win32Api.GetWindowTextLength(hWnd) + 1);
            Win32Api.GetWindowText(hWnd, lpString, lpString.Capacity);
            if (!lpString.ToString().Contains("Viber"))
            {
                return IntPtr.Zero;
            }

            State = ViberState.Start;
            return hWnd;
        }

        public IntPtr Restart()
        {
            StopIfRunning();

            return Start();
        }

        public void StopIfRunning()
        {
            while (IsRunning())
            {
                try
                {
                    Process[] processes = Process.GetProcessesByName(ProcessName);
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
                catch
                { }
            }
            State = ViberState.Stop;
        }

        public bool IsMessageSyncShow()
        {
            var color = GetPixelColor(_positionMap[Position.MessageSyncTitle]);
            return color.HasValue && color.Value.ToArgb() == _positionMap[Position.MessageSyncTitle].Argb;
        }

        public void CloseMessageSync()
        {
            Thread.Sleep(500);
            _virtualMouse.MoveTo(_positionMap[Position.MessageSyncClose]).LeftClick();
            Thread.Sleep(500);
            _virtualMouse.MoveTo(_positionMap[Position.MessageSyncOk]).LeftClick();
        }

        public static bool IsOpen()
        {
            return Win32Api.FindWindow("Qt5QWindowOwnDCIcon", IntPtr.Zero) != IntPtr.Zero;
        }

        private static bool IsRunning()
        {
            return Process.GetProcessesByName(ProcessName).Length > 0;
        }

        private static IntPtr FindWindow(int countAttempts = 3)
        {
            IntPtr hWnd = Win32Api.FindWindow("Qt5QWindowOwnDCIcon", IntPtr.Zero);
            for (int i = 0; i <= countAttempts; i++)
            {
                if (hWnd != IntPtr.Zero) break;
                Thread.Sleep(100);
                hWnd = Win32Api.FindWindow("Qt5QWindowOwnDCIcon", IntPtr.Zero);
            }

            return hWnd;
        }

        private static Color? GetPixelColor(Point point)
        {
            IntPtr hWnd = FindWindow();
            if (hWnd == IntPtr.Zero)
            {
                return null;
            }
            IntPtr hdc = Win32Api.GetDC(IntPtr.Zero);
            uint pixel = Win32Api.GetPixel(hdc, point.X, point.Y);
            Win32Api.ReleaseDC(IntPtr.Zero, hdc);
            Color? color = Color.FromArgb((int)(pixel & 0x000000FF),
                            (int)(pixel & 0x0000FF00) >> 8,
                            (int)(pixel & 0x00FF0000) >> 16);
            return color;
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
                {
                    dsProcRootWindows.Add(hWnd);
                }
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

            return true;
        }

        private static readonly Dictionary<Position, Point> _positionMap = new Dictionary<Position, Point>
        {
            { Position.More, new Point { X = 256, Y = 120, Argb = -1118482 } },
            { Position.PhoneNumberMenu, new Point { X = 200, Y = 400, Argb = -1118739 } },
            { Position.PhoneNumberEdit, new Point { X = 163, Y = 163, Argb = -197380 } },
            { Position.MessageButton, new Point { X = 230, Y = 520, Argb = -10134357 } },
            { Position.MessageEdit, new Point { X = 600, Y = 583, Argb = -2235934 } },
            { Position.MessageEditBlock, new Point { X = 310, Y = 583, Argb = -855053 } },
            { Position.DialogChat, new Point { X = 320, Y = 200, Argb = -1 } },
            { Position.MessageSyncTitle, new Point { X = 692, Y = 186, Argb = -8489518 } },
            { Position.MessageSyncClose, new Point { X = 339, Y = 184, Argb = -1 } },
            { Position.MessageSyncOk, new Point { X = 619, Y = 396, Argb = -1 } }
        };

        #region Dispose pattern
        bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
            }

            //StopIfRunning();

            disposed = true;
        }

        ~Viber()
        {
            Dispose(false);
        }
        #endregion
    }
}
