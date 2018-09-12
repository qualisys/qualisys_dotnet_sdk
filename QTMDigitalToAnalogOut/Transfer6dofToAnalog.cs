using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using QTMRealTimeSDK.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalIO;
using MccDaq;
using AnalogIO;
using ErrorDefs;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;

namespace QTMDigitalToAnalogOut
{
    public class Transfer6dofToAnalog
    {
        public Transfer6dofToAnalog(string ipAddress, string pathToSettings)
        {
            if (ipAddress != null)
                IpAddress = ipAddress;
            if (pathToSettings != null)
                PathToSettings = pathToSettings;
        }
        private string IpAddress = "127.0.0.1";
        private string PathToSettings = "QTMDigitalToAnalogOut.settings.xml";
        private RTProtocol mRtProtocol;

        class BoardInfo
        {
            internal string mDeviceId;
            internal MccDaq.MccBoard mBoard;
            internal MccDaq.Range mRange;
            internal IntPtr mMemHandle;
            internal int mResolution;
            internal int mNumberOfChannels;
            internal ushort[] mDataBuffer;
        }

        List<BoardInfo> mBoards = new List<BoardInfo>();
        Dictionary<string, BoardInfo> mBoardsMap = new Dictionary<string, BoardInfo>();

        public void Start()
        {
            try
            {
                LoadSettings();

                clsErrorDefs.ReportError = MccDaq.ErrorReporting.PrintFatal;
                clsErrorDefs.HandleError = MccDaq.ErrorHandling.DontStop;
                MccDaq.ErrorInfo ULStat = MccDaq.MccService.ErrHandling(clsErrorDefs.ReportError, clsErrorDefs.HandleError);

                MccService.ErrHandling(ErrorReporting.PrintAll, ErrorHandling.DontStop);
                MccDaq.DaqDeviceManager.IgnoreInstaCal();

                MccDaq.DaqDeviceDescriptor[] devices = MccDaq.DaqDeviceManager.GetDaqDeviceInventory(MccDaq.DaqDeviceInterface.Any);
                int boardNumber = 0;
                foreach (var device in devices)
                {
                    // Don't initialize board that we don't want to use since that disrupts any operation on it
                    bool initializeBoard = false;
                    Debug.WriteLine("Found board: " + device.UniqueID);
                    foreach (var setting in mSettings)
                    {
                        if (setting.BoardId == device.UniqueID)
                        {
                            Debug.WriteLine("Initiating board: " + device.UniqueID);
                            initializeBoard = true;
                            break;
                        }
                    }

                    boardNumber++;

                    if (!initializeBoard)
                        continue;

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
                        mMemHandle = MccDaq.MccService.WinBufAllocEx(numberOfChannels * NumberOfOutputValuesPerSecond),
                        mResolution = resolution,
                        mNumberOfChannels = numberOfChannels,
                        mDataBuffer = new ushort[numberOfChannels * NumberOfOutputValuesPerSecond]
                    };
                    mBoards.Add(boardInfo);
                    mBoardsMap.Add(boardInfo.mDeviceId, boardInfo);


                }
            }
            catch (ULException ule)
            {
                Console.WriteLine(ule.Message);
            }

            mRtProtocol = new RTProtocol();

/*
            // TODO::: Read settings from xml file.
            Settings6dofToAnalog settings6dofToAnalog = new Settings6dofToAnalog()
            {
                BoardId = "19D4A6D",
                Channel = 0,
                DataType = SixDofDataType.PositionX,
                Name = "New Body #1",
                MinimumValueInput = 0,
                MaximumValueInput = 1000,
                MinimumValueOutput = -10,
                MaximumValueOutput = 10
            };
            Settings6dofToAnalog settings6dofToAnalog1 = new Settings6dofToAnalog()
            {
                BoardId = "19D4A6D",
                Channel = 1,
                DataType = SixDofDataType.Residual,
                Name = "New Body #1",
                MinimumValueInput = 0,
                MaximumValueInput = 2,
                MinimumValueOutput = -10,
                MaximumValueOutput = 10,
            };
            Settings6dofToAnalog settings6dofToAnalog2 = new Settings6dofToAnalog()
            {
                BoardId = "19D4A6D",
                Channel = 2,
                DataType = SixDofDataType.Existent,
                Name = "New Body #1",
                MinimumValueInput = 0,
                MaximumValueInput = 10,
                MinimumValueOutput = -5,
                MaximumValueOutput = 5,
            };
            settings6dofToAnalog.CalculateStartValues();
            settings6dofToAnalog1.CalculateStartValues();
            settings6dofToAnalog2.CalculateStartValues();
            mSettings6dofToAnalog.Add(settings6dofToAnalog.Name, settings6dofToAnalog);
            mSettings6dofToAnalog.Add(settings6dofToAnalog1.Name, settings6dofToAnalog1);
            mSettings6dofToAnalog.Add(settings6dofToAnalog2.Name, settings6dofToAnalog2);
            mSettings.Add(settings6dofToAnalog);
            mSettings.Add(settings6dofToAnalog1);
            mSettings.Add(settings6dofToAnalog2);

            SaveSettings();
            
            mSettings6dofToAnalog.Clear();
            mSettings.Clear();
*/
        }

        private void ReleaseDAQDevices()
        {
            foreach (var boardInfo in mBoards)
            {
                // Release resources associated with the specified board number within the Universal Library with cbReleaseDaqDevice()
                //    Parameters:
                //    	MccBoard:			Board object

                MccDaq.MccService.WinBufFreeEx(boardInfo.mMemHandle);
                MccDaq.DaqDeviceManager.ReleaseDaqDevice(boardInfo.mBoard);
            }
        }

        public void Stop()
        {
            ReleaseDAQDevices();

            mRtProtocol.Disconnect();
            mRtProtocol.Dispose();
        }

        public bool debugOutput = true;

       

        private Dictionary<string, Settings6DOF> mSettings6D;
        private List<Settings6DOF> mSettings6DList;

        private void Get6dofSettingsFromRT()
        {
            mSettings6D = mRtProtocol.Settings6DOF.Bodies.ToDictionary(a => a.Name);
            mSettings6DList = mRtProtocol.Settings6DOF.Bodies;
        }

        public enum SixDofDataType
        {
            PositionX,
            PositionY,
            PositionZ,
            RotationFirst,
            RotationSecond,
            RotationThird,
            Residual,
            Existent,
        }

        public class Settings6dofToAnalog
        {
            public string Name;
            public SixDofDataType DataType;
            public string BoardId;
            public int Channel;

            public float MinimumValueInput;
            public float MaximumValueInput;
            public float MinimumValueOutput;
            public float MaximumValueOutput;

            private float mafk;
            private float mafm;
            public void CalculateStartValues()
            {
                mafk = ((MaximumValueOutput - MinimumValueOutput) / (MaximumValueInput - MinimumValueInput));
                mafm = (MinimumValueOutput - (mafk * MinimumValueInput));
            }

            public float CalculateAdjustedValue(float value)
            {
                return mafk * value + mafm;
            }
        }

        List<Settings6dofToAnalog> mSettings = new List<Settings6dofToAnalog>();
        MultiValueDictionary<string, Settings6dofToAnalog> mSettings6dofToAnalog = new MultiValueDictionary<string, Settings6dofToAnalog>();
        private const int NumberOfOutputValuesPerSecond = 10;

        

        void SaveSettings()
        {
            try
            {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.Replace
                };
                using (XmlWriter writer = XmlWriter.Create(PathToSettings, xmlWriterSettings))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Settings6dofToAnalog>));
                    xmlSerializer.Serialize(writer, mSettings);
                }
            }
            catch (Exception)
            {
            }
        }
        
        void LoadSettings()
        {
            try
            {
                using (XmlReader reader = new XmlTextReader(PathToSettings))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Settings6dofToAnalog>));
                    mSettings = (List<Settings6dofToAnalog>)xmlSerializer.Deserialize(reader);
                    foreach (var setting in mSettings)
                    {
                        setting.CalculateStartValues();
                        mSettings6dofToAnalog.Add(setting.Name.ToLower(), setting);
                    }
                }
            }
            catch (Exception)
            {
                // Ignore any kind of error and use default settings
            }
        }

        //Random random = new Random();
        //bool started = false;

        public void TransferData()
        {
            if (!mRtProtocol.IsConnected())
            {
                if (!mRtProtocol.Connect(IpAddress, 15425))
                {
                    Console.WriteLine("QTM: Trying to connect");
                    Thread.Sleep(1000);
                    return;
                }
                Console.WriteLine("QTM: Connected");
            }

            if (mRtProtocol.Settings6DOF == null)
            {
                if (!mRtProtocol.Get6dSettings())
                {
                    Console.WriteLine("QTM: Trying to get 6DOF settings");
                    Thread.Sleep(500);
                    return;
                }
                Console.WriteLine("QTM: 6DOF data available");

                Get6dofSettingsFromRT();
                mRtProtocol.StreamAllFrames(QTMRealTimeSDK.Data.ComponentType.Component6dEulerResidual, 15425);
                Console.WriteLine("QTM: Starting to stream data");
                Thread.Sleep(500);
            }

            PacketType packetTypeTemp;
            PacketType packetType;
            while (true)
            {
                Console.WriteLine(mRtProtocol.ReceiveRTPacket(out packetTypeTemp, false));
                Console.WriteLine(mRtProtocol.GetErrorString());
                packetType = packetTypeTemp;
                break;
                
            }


            if (packetType == PacketType.PacketData)
            {
                if (debugOutput)
                {
                    Console.Clear();
                }
                var sixDofData = mRtProtocol.GetRTPacket().Get6DOFEulerResidualData();
                if (sixDofData != null)
                {
                    for (int i = 0; i < sixDofData.Count; i++)
                    {
                        var sixDof = sixDofData[i];

                        var settings = mSettings6dofToAnalog.GetValues(mSettings6DList[i].Name.ToLower(), false);
                        if (settings == null)
                            continue;

                        if (debugOutput)
                        {
                            var frameNumber = mRtProtocol.GetRTPacket().Frame;
                            Console.WriteLine("Frame:{0:D5} X:{1:F2} Y:{2:F2} Z:{3:F2} 1:{4:F2} 2:{5:F2} 3:{6:F2} R:{7:F2}",
                                frameNumber.ToString(), sixDof.Position.X, sixDof.Position.Y, sixDof.Position.Z, sixDof.Rotation.First, sixDof.Rotation.Second, sixDof.Rotation.Third, sixDof.Residual);
                        }

                        MccDaq.ErrorInfo errorInfo;

                        foreach (var setting in settings)
                        {
                            BoardInfo boardInfo;
                            if (mBoardsMap.TryGetValue(setting.BoardId, out boardInfo))
                            {
                                // Do calculation to analog value... and send it out via the analog board
                                float attributeValue = 0.0f;

                                if (float.IsNaN(sixDof.Position.X))
                                {
                                    if (setting.DataType != SixDofDataType.Existent)
                                    {
                                        // Ignore missing values, continue to output the previous value for datatypes other than existent
                                        continue;
                                    }
                                }

                                switch (setting.DataType)
                                {
                                    case SixDofDataType.PositionX:
                                        attributeValue = sixDof.Position.X;
                                        break;
                                    case SixDofDataType.PositionY:
                                        attributeValue = sixDof.Position.Y;
                                        break;
                                    case SixDofDataType.PositionZ:
                                        attributeValue = sixDof.Position.Z;
                                        break;
                                    case SixDofDataType.RotationFirst:
                                        attributeValue = sixDof.Rotation.First;
                                        break;
                                    case SixDofDataType.RotationSecond:
                                        attributeValue = sixDof.Rotation.Second;
                                        break;
                                    case SixDofDataType.RotationThird:
                                        attributeValue = sixDof.Rotation.Third;
                                        break;
                                    case SixDofDataType.Residual:
                                        attributeValue = sixDof.Residual;
                                        break;
                                    case SixDofDataType.Existent:
                                        attributeValue = float.IsNaN(sixDof.Position.X) ? setting.MinimumValueInput : setting.MaximumValueInput;
                                        break;
                                    default:
                                        break;
                                }

                                if (attributeValue <= setting.MinimumValueInput)
                                {
                                    attributeValue = setting.MinimumValueInput;
                                }
                                else if (attributeValue >= setting.MaximumValueInput)
                                {
                                    attributeValue = setting.MaximumValueInput;
                                }
                                var volt = setting.CalculateAdjustedValue(attributeValue);
                                
                                if (debugOutput)
                                {
                                    Console.WriteLine("Device: {5} Channel: {0} Body: {1} DataType: {2} => value: {3:F2} => volt: {4:F2}", setting.Channel, setting.Name, setting.DataType, attributeValue, volt , setting.BoardId);
                                }
                                var board = mBoards.Find(x => x.mDeviceId == setting.BoardId);
                                board.mBoard.FromEngUnits(Range.Bip10Volts, setting.CalculateAdjustedValue(attributeValue), out ushort dataValue);
                                errorInfo = boardInfo.mBoard.AOut(setting.Channel, MccDaq.Range.Bip10Volts, dataValue);
                            }

                        }
                    }
                }
            }
            else if (packetType == PacketType.PacketEvent)
            {
                // If an event comes from QTM then print it out
                var qtmEvent = mRtProtocol.GetRTPacket().GetEvent();
                if (qtmEvent == QTMEvent.EventConnected ||
                    qtmEvent == QTMEvent.EventConnectionClosed ||
                    qtmEvent == QTMEvent.EventRTFromFileStarted ||
                    qtmEvent == QTMEvent.EventRTFromFileStopped ||
                    qtmEvent == QTMEvent.EventCaptureStarted ||
                    qtmEvent == QTMEvent.EventCaptureStopped)
                {
                    mRtProtocol.ClearSettings();
                }
                if (debugOutput)
                {
                    Console.WriteLine("Event: {0}", qtmEvent);
                }
            }
        }
    }
    
}