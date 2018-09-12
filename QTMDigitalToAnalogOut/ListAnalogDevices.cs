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
    public class ListAnalogDevices
    {
        public ListAnalogDevices()
        {

        }

        public void list()
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

    }
}
