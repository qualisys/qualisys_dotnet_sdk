using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace QTMDigitalToAnalogOut
{
    class Program
    {
        [Verb("stream", HelpText = "Stream data to analog device")]
        class StreamOptions
        {
            [Option("ip", Required = false, HelpText = "Ip address of computer running QTM.", Default = Defaults.IpAddress)]
            public string IpAddress { get; set; }
            [Option("settings", Required = false, HelpText = "Settings file for 6dof to analog calculations.", Default = Defaults.PathToSettings)]
            public string PathToSettings { get; set; }
            [Option("output", Required = false, HelpText = "Set to output verbose messages.", Default = true)]
            public bool DebugOutput { get; set; }
        }

        [Verb("list", HelpText = "List connected devices")]
        class ListOptions { }

        [Verb("test", HelpText = "Test devices")]
        class TestOptions { }
        
        private static void Main(string[] args)
        {
            DisableConsoleQuickEdit.Run();

            Parser.Default.ParseArguments<StreamOptions ,ListOptions, TestOptions>(args)
                .MapResult(
                    (StreamOptions opts) => StreamData(opts),
                    (ListOptions opts) => ListDevices(opts),
                    (TestOptions opts) => TestDevices(opts),
                    errs => 1);

            int StreamData(StreamOptions options)
            {
                Transfer6dofToAnalog transfer6dofToAnalog = new Transfer6dofToAnalog(options.IpAddress, options.PathToSettings, options.DebugOutput);
                transfer6dofToAnalog.Start();
                while (true)
                {
                    transfer6dofToAnalog.TransferData();
                    if (Console.KeyAvailable)
                    {
                        if (Console.ReadKey(false).Key == ConsoleKey.Escape)
                            break;
                    }
                }
                transfer6dofToAnalog.Stop();
                return 0;
            }

            int ListDevices(ListOptions options)
            {
                Devices.List();
                return 0;
            }

            int TestDevices(TestOptions options)
            {
                Devices.Test();
                return 0;
            }
        }
    }
}