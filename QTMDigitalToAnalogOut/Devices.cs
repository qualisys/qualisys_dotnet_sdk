using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MccDaq;
using AnalogIO;
using ErrorDefs;

namespace QTMDigitalToAnalogOut
{
    public class Devices
    {
        class BoardInfo
        {
            internal string mDeviceId;
            internal MccDaq.MccBoard mBoard;
            internal MccDaq.Range mRange;
            internal int mResolution;
            internal int mNumberOfChannels;
        }

        static public void List()
        {
            clsErrorDefs.ReportError = MccDaq.ErrorReporting.PrintFatal;
            clsErrorDefs.HandleError = MccDaq.ErrorHandling.DontStop;
            MccDaq.ErrorInfo ULStat = MccDaq.MccService.ErrHandling(clsErrorDefs.ReportError, clsErrorDefs.HandleError);

            MccService.ErrHandling(ErrorReporting.PrintAll, ErrorHandling.DontStop);
            MccDaq.DaqDeviceManager.IgnoreInstaCal();

            MccDaq.DaqDeviceDescriptor[] devices = MccDaq.DaqDeviceManager.GetDaqDeviceInventory(MccDaq.DaqDeviceInterface.Any);
            int boardNumber = 1;
            Console.WriteLine("Found {0} Mcc devices", devices.Length);
            Console.WriteLine("--------------------------------");
            foreach (var device in devices)
            {
                Console.WriteLine("Device: " + boardNumber);
                MccDaq.MccBoard daqBoard = MccDaq.DaqDeviceManager.CreateDaqDevice(boardNumber++, device);
                int ChannelType = clsAnalogIO.ANALOGOUTPUT;
                var analogIO = new clsAnalogIO();
                MccDaq.Range Range;
                int resolution;
                int LowChan;
                MccDaq.TriggerType DefaultTrig;
                var numberOfChannels = analogIO.FindAnalogChannelsOfType(daqBoard, ChannelType, out resolution, out Range, out LowChan, out DefaultTrig);
                string uniqueId;
                daqBoard.BoardConfig.GetDeviceUniqueId(out uniqueId);

                Console.WriteLine("Found board: " + daqBoard.BoardName);
                Console.WriteLine("Board ID: " + device.UniqueID);
                Console.WriteLine("Number of D/A channels: " + numberOfChannels);
                Console.WriteLine("Resolution: " + resolution);
                Console.WriteLine("First channel: " + LowChan);
                Console.WriteLine("Range: " + Range);
                Console.WriteLine("--------------------------------");
            }
        }

        static public void Test()
        {
            try
            {
                List<BoardInfo> boards = new List<BoardInfo>();

                clsErrorDefs.ReportError = MccDaq.ErrorReporting.PrintFatal;
                clsErrorDefs.HandleError = MccDaq.ErrorHandling.DontStop;
                MccDaq.ErrorInfo ULStat = MccDaq.MccService.ErrHandling(clsErrorDefs.ReportError, clsErrorDefs.HandleError);

                MccService.ErrHandling(ErrorReporting.PrintAll, ErrorHandling.DontStop);
                MccDaq.DaqDeviceManager.IgnoreInstaCal();

                MccDaq.DaqDeviceDescriptor[] devices = MccDaq.DaqDeviceManager.GetDaqDeviceInventory(MccDaq.DaqDeviceInterface.Any);
                int boardNumber = 0;
                foreach (var device in devices)
                {
                    boardNumber++;
                    MccDaq.MccBoard daqBoard = MccDaq.DaqDeviceManager.CreateDaqDevice(boardNumber++, device);

                    // Find any analog board with analog output possibilities
                    int ChannelType = clsAnalogIO.ANALOGOUTPUT;
                    var analogIO = new clsAnalogIO();

                    MccDaq.Range Range;
                    int resolution;
                    int LowChan;
                    MccDaq.TriggerType DefaultTrig;
                    var numberOfChannels = analogIO.FindAnalogChannelsOfType(daqBoard, ChannelType, out resolution, out Range, out LowChan, out DefaultTrig);
                    if (numberOfChannels <= 0)
                    {
                        MccDaq.DaqDeviceManager.ReleaseDaqDevice(daqBoard);
                        continue;
                    }

                    // Set aside memory to hold D/A data
                    string uniqueId;
                    daqBoard.BoardConfig.GetDeviceUniqueId(out uniqueId);
                    BoardInfo boardInfo = new BoardInfo()
                    {
                        mDeviceId = uniqueId.ToUpper(),
                        mBoard = daqBoard,
                        mRange = Range,
                        mResolution = resolution,
                        mNumberOfChannels = numberOfChannels,
                    };
                    boards.Add(boardInfo);
                }

                bool outputChannelNumbers = false;
                MccDaq.ErrorInfo errorInfo;
                int outputValue = 0;
                while (true)
                {
                    outputValue = 0;
                    Console.Clear();
                    if (outputChannelNumbers)
                    {
                        foreach (var board in boards)
                        {
                            Console.WriteLine("Voltage on each channel is set to the channel number", board.mDeviceId, outputValue);

                            for (int channel = 0; channel < board.mNumberOfChannels; channel++)
                            {
                                board.mBoard.FromEngUnits(Range.Bip10Volts, channel, out ushort dataValue);
                                errorInfo = board.mBoard.AOut(channel, MccDaq.Range.Bip10Volts, dataValue);
                            }
                        }
                    }
                    else
                    {
                        foreach (var board in boards)
                        {
                            outputValue++;
                            Console.WriteLine("Voltage on device {0} is set to {1}", board.mDeviceId, outputValue);
                            board.mBoard.FromEngUnits(Range.Bip10Volts, outputValue, out ushort dataValue);
                            for (int channel = 0; channel < board.mNumberOfChannels; channel++)
                            {
                                errorInfo = board.mBoard.AOut(channel, MccDaq.Range.Bip10Volts, dataValue);
                            }
                        }
                    }
                    Console.WriteLine("Toggle output values with Spacebar and exit with Escape");

                    var keyPressed = Console.ReadKey(false).Key;
                    if (keyPressed == ConsoleKey.Escape)
                    {
                        break;
                    }
                    else if (keyPressed == ConsoleKey.Spacebar)
                    {
                        outputChannelNumbers = !outputChannelNumbers;
                    }

                    foreach (var board in boards)
                    {
                        outputValue++;
                        Console.WriteLine("Voltage on device is set to 0");
                        board.mBoard.FromEngUnits(Range.Bip10Volts, 0, out ushort dataValue);
                        for (int channel = 0; channel < board.mNumberOfChannels; channel++)
                        {
                            errorInfo = board.mBoard.AOut(channel, MccDaq.Range.Bip10Volts, dataValue);
                        }
                    }
                }

                foreach (var board in boards)
                {
                    MccDaq.DaqDeviceManager.ReleaseDaqDevice(board.mBoard);
                }

                Console.WriteLine();
            }
            catch (ULException ule)
            {
                Console.WriteLine(ule.Message);
            }
}
    }
}
