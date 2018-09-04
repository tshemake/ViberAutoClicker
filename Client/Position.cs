using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public struct Point
    {
        public int X;
        public int Y;
        public int Argb;
    }

    public enum Position
    {
        More,
        PhoneNumberMenu,
        PhoneNumberEdit,
        MessageButton,
        MessageEdit,
        MessageEditBlock,
        DialogChat
    }
}
