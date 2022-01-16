using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace ReactiveUI.UwpRouting
{
    public static class MainThread
    {
        public static bool IsMainThread
        {
            get
            {
                // if there is no main window, then this is either a service
                // or the UI is not yet constructed, so the main thread is the
                // current thread
                try
                {
                    if (CoreApplication.MainView?.CoreWindow == null)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw;
                    //DiagnosticsHelpers.ReportProblem($"Exception thrown while trying to validate MainView creation. Details to follow.", DiagnosticsHelpers.ProblemSeverity.Error, ex);
                    //return true;
                }

                return CoreApplication.MainView.CoreWindow.Dispatcher?.HasThreadAccess ?? false;
            }
        }

        public static CoreDispatcher GetCoreDispatcherForMainThread()
        {
            return CoreApplication.MainView?.CoreWindow?.Dispatcher;
        }

        // Note: The two RunTaskAsync extension methods come from here (MIT License):
        // https://github.com/Microsoft/Windows-task-snippets/blob/master/tasks/UI-thread-task-await-from-background-thread.md
        // This should probably all be converted to use DispatcherQueue; see: https://docs.microsoft.com/en-us/windows/communitytoolkit/extensions/dispatcherqueueextensions
        public static async Task<T> RunTaskAsync<T>(this CoreDispatcher dispatcher, Func<Task<T>> func, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();
            await dispatcher.RunAsync(priority, async () =>
            {
                try
                {
                    taskCompletionSource.SetResult(await func());
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });
            return await taskCompletionSource.Task;
        }

        // There is no TaskCompletionSource<void> so we use a bool that we throw away.
        public static async Task RunTaskAsync(this CoreDispatcher dispatcher,
            Func<Task> func, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal) =>
            await RunTaskAsync(dispatcher, async () => { await func(); return false; }, priority);

    }
}
