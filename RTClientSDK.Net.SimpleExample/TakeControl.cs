using System;
using System.Collections.Generic;
using System.Threading;
// Consume namespaces from RTClientSDK.dll
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;

namespace RTSDKExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Example example = new Example("127.0.0.1", "password", @"filetoload.qtm");
            while (true)
            {
                // Define the IP address of the computer running QTM
                example.HandleStreaming();
                if (Console.KeyAvailable)
                {
                    if (Console.ReadKey(false).Key == ConsoleKey.Escape)
                        break;
                }
            }
        }
    }

    class Example
    {
        // Create a realtime streaming protocol object that talks to the QTM server
        private RTProtocol rtProtocol = new RTProtocol();

        private string ipAddress;
        private string password;
        private string filename;

        public Example(string ipAddress, string password, string filename)
        {
            this.ipAddress = ipAddress;
            this.password = password;
            this.filename = filename;
        }

        public void HandleStreaming()
        {
            // Check if there is a connection with QTM
            if (!rtProtocol.IsConnected())
            {
                // If not connected, establish a connection
                if (!rtProtocol.Connect(ipAddress))
                {
                    Console.WriteLine("QTM: Trying to connect");
                    Thread.Sleep(1000);
                    return;
                }
                Console.WriteLine("QTM: Connected");

                // Take control of QTM and load the desired file and start the realtime stream
                if (rtProtocol.TakeControl(password))
                {
                    Console.WriteLine("QTM: Took control of QTM using specified password in Options/Real-Time Output.");
                    rtProtocol.LoadFile(filename);
                    rtProtocol.StartCapture(true);
                }
                else
                {
                    Console.WriteLine("QTM: Failed to take control of QTM using specified password in Options/Real-Time Output.");
                }
            }

            // Check for available 6DOF rigid body data in the stream
            if (rtProtocol.Settings6DOF == null)
            {
                if (!rtProtocol.Get6dSettings())
                {
                    Console.WriteLine("QTM: Trying to get 6DOF settings");
                    Thread.Sleep(500);
                    return;
                }
                Console.WriteLine("QTM: 6DOF data available");

                // If 6DOF was not streaming tell QTM to give the data as fast as possible
                rtProtocol.StreamAllFrames(QTMRealTimeSDK.Data.ComponentType.Component6dEulerResidual);
                Console.WriteLine("QTM: Starting to stream 6DOF data");
                Thread.Sleep(500);
            }

            // Get RTPacket from stream
            PacketType packetType;
            rtProtocol.Receive(out packetType, false);

            // Handle 6DOF rigid body data
            if (packetType == PacketType.PacketData)
            {
                var sixDofData = rtProtocol.GetRTPacket().Get6DOFEulerResidualData();
                if (sixDofData != null)
                {
                    // Print out the available 6DOF data.
                    for (int body = 0; body < sixDofData.Count; body++)
                    {
                        var sixDofBody = sixDofData[body];
                        var bodySetting = rtProtocol.Settings6DOF.Bodies[body];
                        if (bodySetting.Enabled)
                        {
                            Console.WriteLine("Frame:{0:D5} Body:{1,20} X:{2,7:F1} Y:{3,7:F1} Z:{4,7:F1} First Angle:{5,7:F1} Second Angle:{6,7:F1} Third Angle:{7,7:F1} Residual:{8,7:F1}",
                                rtProtocol.GetRTPacket().Frame,
                                bodySetting.Name,
                                sixDofBody.Position.X, sixDofBody.Position.Y, sixDofBody.Position.Z,
                                sixDofBody.Rotation.First, sixDofBody.Rotation.Second, sixDofBody.Rotation.Third,
                                sixDofBody.Residual);
                        }
                    }
                }
            }

            // Handle event packet
            if (packetType == PacketType.PacketEvent)
            {
                // If an event comes from QTM then print it out
                var qtmEvent = rtProtocol.GetRTPacket().GetEvent();
                Console.WriteLine("{0}", qtmEvent);
            }
        }

        ~Example()
        {
            if (rtProtocol.IsConnected())
            {
                rtProtocol.StreamFramesStop();
                rtProtocol.Disconnect();
            }
        }
    }
}