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
    public class TestDevices
    {
        class BoardInfo
        {
            internal string mDeviceId;
            internal MccDaq.MccBoard mBoard;
            internal MccDaq.Range mRange;
            internal int mResolution;
            internal int mNumberOfChannels;
        }

        List<BoardInfo> mBoards = new List<BoardInfo>();
        Dictionary<string, BoardInfo> mBoardsMap = new Dictionary<string, BoardInfo>();
        public TestDevices()
        {

        }
        public void test()
        {
            try
            {
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
                    mBoards.Add(boardInfo);
                    mBoardsMap.Add(boardInfo.mDeviceId, boardInfo);
                }
            }

            catch (ULException ule)
            {
                Console.WriteLine(ule.Message);
            }
            bool outputChannelNumbers = false;
            MccDaq.ErrorInfo errorInfo;
            int outPutValue = 0;
            while (true)
            {
                outPutValue = 0;
                Console.Clear();
                if (outputChannelNumbers)
                {
                    foreach (var board in mBoards)
                    {
                        Console.WriteLine("Voltage on each channel is set to the channel number", board.mDeviceId, outPutValue);

                        for (int channel = 0; channel < board.mNumberOfChannels; channel++)
                        {
                            board.mBoard.FromEngUnits(Range.Bip10Volts, channel, out ushort dataValue);
                            errorInfo = board.mBoard.AOut(channel, MccDaq.Range.Bip10Volts, dataValue);
                        }
                    }
                }
                else
                {
                    foreach (var board in mBoards)
                    {
                        outPutValue++;
                        Console.WriteLine("Voltage on device {0} is set to {1}", board.mDeviceId, outPutValue);
                        board.mBoard.FromEngUnits(Range.Bip10Volts, outPutValue, out ushort dataValue);
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

                foreach (var board in mBoards)
                {
                    outPutValue++;
                    Console.WriteLine("Voltage on device is set to 0");
                    board.mBoard.FromEngUnits(Range.Bip10Volts, 0, out ushort dataValue);
                    for (int channel = 0; channel < board.mNumberOfChannels; channel++)
                    {
                        errorInfo = board.mBoard.AOut(channel, MccDaq.Range.Bip10Volts, dataValue);
                    }
                }
            }

            foreach (var board in mBoards)
            {
                MccDaq.DaqDeviceManager.ReleaseDaqDevice(board.mBoard);
            }
            Console.WriteLine("");

        }
    }


}
