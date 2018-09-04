using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public static class TaskExtensions
    {
        public static Task<T> StartNew<T>(this TaskFactory t, Func<T> func, ApartmentState state)
        {
            var tcs = new TaskCompletionSource<T>();
            var thread = new Thread(() => {
                try
                {
                    tcs.SetResult(func());
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.IsBackground = true;
            thread.SetApartmentState(state);
            thread.Start();
            return tcs.Task;
        }

        public static Task StartNew(this TaskFactory t, Action action, ApartmentState state)
        {
            var tcs = new TaskCompletionSource<bool>();
            var thread = new Thread(() => {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.IsBackground = true;
            thread.SetApartmentState(state);
            thread.Start();
            return tcs.Task;
        }
    }
}
