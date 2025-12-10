using InfoPanel;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace WinUISingleInstanceApplication
{
    public static class SingleInstanceApplication
    {
        private static bool AlreadyProcessedOnThisInstance;

        internal static void Make(string appName, bool uniquePerUser = true)
        {
            if (AlreadyProcessedOnThisInstance)
            {
                return;
            }
            AlreadyProcessedOnThisInstance = true;

            Application app = Application.Current;

            string eventName = uniquePerUser
                ? $"{appName}-{Environment.MachineName}-{Environment.UserDomainName}-{Environment.UserName}"
                : $"{appName}-{Environment.MachineName}";

            bool isSecondaryInstance = true;

            EventWaitHandle eventWaitHandle = null;
            try
            {
                eventWaitHandle = EventWaitHandle.OpenExisting(eventName);
            }
            catch
            {
                // This code only runs on the first instance.
                isSecondaryInstance = false;
            }

            if (isSecondaryInstance)
            {
                ActivateFirstInstanceWindow(eventWaitHandle);
                // Let's produce a non-interceptable exit.
                Environment.Exit(0);
            }

            RegisterFirstInstanceWindowActivation(app, eventName);
        }

        private static void ActivateFirstInstanceWindow(EventWaitHandle eventWaitHandle)
        {
            // Let's notify the first instance to activate its main window.
            _ = eventWaitHandle.Set();
        }

        private static void RegisterFirstInstanceWindowActivation(Application app, string eventName)
        {
            EventWaitHandle eventWaitHandle = new EventWaitHandle(
                false,
                EventResetMode.AutoReset,
                eventName);

            _ = ThreadPool.RegisterWaitForSingleObject(eventWaitHandle, WaitOrTimerCallback, app, Timeout.Infinite, false);

            eventWaitHandle.Close();
        }

        private static void WaitOrTimerCallback(object state, bool timedOut)
        {
            Application application = (Application)state;
            if (application is App app)
            {
                // Use WinUI 3 DispatcherQueue
                App.MainDispatcherQueue?.TryEnqueue(() =>
                {
                    //app.ShowMainWindow();
                });
            }
        }
    }
}