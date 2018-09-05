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
        private const string _processName = "Viber";

        private readonly Mouse _virtualMouse;

        public int State { get; private set; }

        public string ExePath { get; private set; }

        public Viber(string path)
        {
            ExePath = path;
            _virtualMouse = new Mouse(0, 0);
        }

        public bool Run()
        {
            StopIfRunning();

            Process.Start(ExePath);
            while (!IsOpen())
            {
                Thread.Sleep(500);
            }
            Thread.Sleep(3000);
            IntPtr hWnd = FindWindow();

            StringBuilder lpString = new StringBuilder(Win32Api.GetWindowTextLength(hWnd) + 1);
            Win32Api.GetWindowText(hWnd, lpString, lpString.Capacity);
            if (!Regex.Match(lpString.ToString(), @"^Viber \+([0-9]{11})$").Success)
            {
                return false;
            }

            SetWindowSize(hWnd);
            State = 0;
            return true;
        }

        public bool Send(string phoneNumber, string message)
        {
            if (!GoToMore()) return false;
            State = 1;
            if (!ClickPhoneNumberMenu()) return false;
            State = 2;
            if (!EnterPhoneNumber(phoneNumber)) return false;
            State = 3;
            if (!ClickMessageButton()) return false;
            State = 4;
            if (!IsEnableSendMessage()) return false;
            State = 5;
            if (!EnterMessage(message)) return false;
            State = 6;
            if (!SendMessage()) return false;
            State = 7;
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

        public bool ClickMessageButton(int countAttempts = 5)
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
            if (!color.HasValue || color.Value.ToArgb() != _positionMap[Position.MessageEditBlock].Argb)
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
            State = 8;
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

        private static void StopIfRunning()
        {
            while (IsRunning())
            {
                try
                {
                    Process[] processes = Process.GetProcessesByName(_processName);
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
        }

        private static bool IsOpen()
        {
            return Win32Api.FindWindow("Qt5QWindowOwnDCIcon", IntPtr.Zero) != IntPtr.Zero;
        }

        private static bool IsRunning()
        {
            return Process.GetProcessesByName(_processName).Length > 0;
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
            if (pixel == 0xFFFFFF) return null;
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
            { Position.MessageEditBlock, new Point { X = 310, Y = 583, Argb = -1315346 } },
            { Position.DialogChat, new Point { X = 320, Y = 200, Argb = -2235934 } }
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
