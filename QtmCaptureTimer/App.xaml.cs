using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace QtmCaptureTimer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            // Read ip address and timecode setting from cmd line
            string ipAddress = String.Empty;
            bool useTimecode = false;
            for (int i = 0; i < e.Args.Length; i++)
            {
                var arg = e.Args[i].ToLower();
                if (arg == "timecode")
                    useTimecode = true;
                else if (arg.Contains("ip"))
                {
                    var ipargs = arg.Split(':');
                    if (ipargs.Length > 1)
                        ipAddress = ipargs[1];
                }
                else if (arg == "help")
                {
                    Help.ShowHelp();
                }
            }

            // Create main application window, starting minimized if specified
            MainWindow mainWindow = new MainWindow(ipAddress, useTimecode);
            mainWindow.Show();

        }
    }
}
