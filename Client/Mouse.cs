using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Client.Native;

namespace Client
{
    class Mouse
    {
        /// <summary>
        /// Устанавливает, что произошло перемещение.
        /// </summary>
        private const int MOUSEEVENTF_MOVE = 0x0001;
        /// <summary>
        /// Устанавливает, что нажата левая кнопка.
        /// </summary>
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        /// <summary>
        /// Устанавливает, что отпущена левая кнопка.
        /// </summary>
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        /// <summary>
        /// Устанавливает, что нажата правая кнопка.
        /// </summary>
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        /// <summary>
        /// Устанавливает, что отпущена правая кнопка.
        /// </summary>
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        /// <summary>
        /// Устанавливает, что нажата средняя кнопка.
        /// </summary>
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        /// <summary>
        /// Устанавливает, что отпущена средняя кнопка.
        /// </summary>
        private const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        /// <summary>
        /// Устанавливает, что параметры  dx и dy содержат нормализованные абсолютные координаты. 
        /// </summary>
        private const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        public Point Position;

        public Mouse(double x, double y)
        {
            Position = new Point(x, y);
        }

        public Mouse(Point mouse)
            : this(mouse.X, mouse.Y)
        {
        }

        public void MoveTo(int xDelta, int yDelta)
        {
            Position.X += xDelta;
            Position.Y += yDelta;
            Win32Api.mouse_event(MOUSEEVENTF_MOVE, xDelta, yDelta, 0, 0);
        }

        public void LeftClick()
        {
            Win32Api.mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            Win32Api.mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        public void LeftClick(int xDelta, int yDelta)
        {
            Win32Api.mouse_event(MOUSEEVENTF_LEFTDOWN, xDelta, yDelta, 0, 0);
            Win32Api.mouse_event(MOUSEEVENTF_LEFTUP, xDelta, yDelta, 0, 0);
        }
    }
}
