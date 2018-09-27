using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Client.Helpers
{
    public sealed class DispatcherHelper
    {
        public static Dispatcher UIDispatcher
        {
            get;
            private set;
        }

        public static void CheckBeginInvokeOnUI(Action action)
        {
            if (action == null)
            {
                return;
            }
            DispatcherHelper.CheckDispatcher();
            if (DispatcherHelper.UIDispatcher.CheckAccess())
            {
                action();
                return;
            }
            DispatcherHelper.UIDispatcher.BeginInvoke(action, new object[0]);
        }

        private static void CheckDispatcher()
        {
            if (DispatcherHelper.UIDispatcher == null)
            {
                StringBuilder stringBuilder = new StringBuilder("The DispatcherHelper is not initialized.");
                stringBuilder.AppendLine();
                stringBuilder.Append("Call DispatcherHelper.Initialize() in the static App constructor.");
                throw new InvalidOperationException(stringBuilder.ToString());
            }
        }

        public static void Initialize()
        {
            if (DispatcherHelper.UIDispatcher != null && DispatcherHelper.UIDispatcher.Thread.IsAlive)
            {
                return;
            }
            DispatcherHelper.UIDispatcher = Dispatcher.CurrentDispatcher;
        }

        public static void Reset()
        {
            DispatcherHelper.UIDispatcher = null;
        }

        public static DispatcherOperation RunAsync(Action action)
        {
            DispatcherHelper.CheckDispatcher();
            return DispatcherHelper.UIDispatcher.BeginInvoke(action, new object[0]);
        }
    }
}
