using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace QTMDigitalToAnalogOut
{
    class Program
    {
        
            [Verb("stream", HelpText = "Stream data to analog device")]
            class StreamOptions
            {
                [Value(0)]
                public string ipAddress { get; set; }
                [Value(1)]
                public string pathToSettings { get; set; }
            }
            [Verb("list", HelpText = "List connected devices")]
            class ListOptions { }
            [Verb("test", HelpText = "Test devices")]
            class TestOptions { }
        
        private 
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments< StreamOptions ,ListOptions, TestOptions> (args)
                .MapResult(
                (StreamOptions opts) => StreamData(opts),
                (ListOptions opts) => ListDevices(opts),
                (TestOptions opts) => TestDevices(opts),
                errs => 1);

            int StreamData(StreamOptions opts)
            {
                Transfer6dofToAnalog transfer6dofToAnalog = new Transfer6dofToAnalog(opts.ipAddress, opts.pathToSettings);
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
            int ListDevices(ListOptions opts)
            {
                ListAnalogDevices listAnalogDevices = new ListAnalogDevices();
                listAnalogDevices.list();
                return 0;
            }
            int TestDevices(TestOptions opts)
            {
                TestDevices testDevices = new TestDevices();
                testDevices.test();
                return 0;
            }
        }
    }


}
