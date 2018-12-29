using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace QEncLogReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Application_DispatcherUnhandledException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show("An unexpected and unrecoverable problem has occourred. \r\nThe software will now exit.\r\n\r\n" + string.Format("Captured an unhandled exception：\r\n{0}\r\n\r\nException Message：\r\n{1}\r\n\r\nException StackTrace：\r\n{2}", ex.GetType(), ex.Message, ex.StackTrace), "The software will now exit.", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(0);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            MessageBox.Show("An unexpected problem has occourred. \r\nSome operation has been terminated.\r\n\r\n" + string.Format("Captured an unhandled exception：\r\n{0}\r\n\r\nException Message：\r\n{1}\r\n\r\nException StackTrace：\r\n{2}", ex.GetType(), ex.Message, ex.StackTrace), "Some operation has been terminated.", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                String projectName = Assembly.GetExecutingAssembly().GetName().Name.ToString();
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(projectName + ".QEncInfo.dll"))
                {
                    Byte[] b = new Byte[stream.Length];
                    stream.Read(b, 0, b.Length);
                    return Assembly.Load(b);
                }
            };
        }
    }
}
