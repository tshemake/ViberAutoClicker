using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public enum ViberState : byte
    {
        Init,
        Start,
        Stop,
        Run,
        GoToMore,
        ClickPhoneNumberMenu,
        EnterPhoneNumber,
        ClickMessageMenu,
        IsEnableSendMessage,
        EnterMessage,
        SendMessage
    }
}
