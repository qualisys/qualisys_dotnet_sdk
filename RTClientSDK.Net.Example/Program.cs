// Realtime SDK Example for Qualisys Track Manager. Copyright 2016 Qualisys AB
//
using System;
using System.Collections.Generic;
using System.Threading;
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;

namespace RTSDKExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Example example = new Example();
            example.DiscoverQTMServers(4547);
            Console.WriteLine("Press key to continue");
            Console.ReadKey();
            while (true)
            {
                example.HandleStreaming("127.0.0.1");
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
        RTProtocol rtProtocol = new RTProtocol();

        public void DiscoverQTMServers(ushort discoveryPort)
        {
            if (rtProtocol.DiscoverRTServers(discoveryPort))
            {
                var discoveryResponses = rtProtocol.DiscoveryResponses;
                foreach (var discoveryResponse in discoveryResponses)
                {
                    Console.WriteLine("Host:{0,20}\tIP Adress:{1,15}\tInfo Text:{2,20}\tCamera count:{3,3}", discoveryResponse.HostName, discoveryResponse.IpAddress, discoveryResponse.InfoText, discoveryResponse.CameraCount);
                }
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

        public void HandleStreaming(string ipAddress)
        {
            // Check if connection to QTM is possible
            if (!rtProtocol.IsConnected())
            {
                if (!rtProtocol.Connect(ipAddress))
                {
                    Console.WriteLine("QTM: Trying to connect");
                    Thread.Sleep(1000);
                    return;
                }
                Console.WriteLine("QTM: Connected");
            }

            // Check for available 6DOF data in the stream
            if (rtProtocol.Settings6DOF == null)
            {
                if (!rtProtocol.Get6DSettings())
                {
                    Console.WriteLine("QTM: Trying to get 6DOF settings");
                    Thread.Sleep(500);
                    return;
                }
                Console.WriteLine("QTM: 6DOF data available");

                rtProtocol.StreamAllFrames(QTMRealTimeSDK.Data.ComponentType.Component6dEulerResidual);
                Console.WriteLine("QTM: Starting to stream 6DOF data");
                Thread.Sleep(500);
            }

            // Get RTPacket from stream
            PacketType packetType;
            rtProtocol.ReceiveRTPacket(out packetType, false);

            // Handle data packet
            if (packetType == PacketType.PacketData)
            {
                var sixDofData = rtProtocol.GetRTPacket().Get6DOFEulerResidualData();
                if (sixDofData != null)
                {
                    // Print out the available 6DOF data.
                    for (int body = 0; body < sixDofData.Count; body++)
                    {
                        var sixDofBody = sixDofData[body];
                        var bodySetting = rtProtocol.Settings6DOF.bodies[body];
                        Console.WriteLine("Frame:{0:D5} Body:{1,20} X:{2,7:F1} Y:{3,7:F1} Z:{4,7:F1} Roll:{5,7:F1} Pitch:{6,7:F1} Yaw:{7,7:F1} Residual:{8,7:F1}",
                            rtProtocol.GetRTPacket().Frame,
                            bodySetting.Name,
                            sixDofBody.Position.X, sixDofBody.Position.Y, sixDofBody.Position.Z,
                            sixDofBody.Rotation.Roll, sixDofBody.Rotation.Pitch, sixDofBody.Rotation.Yaw,
                            sixDofBody.Residual);
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
    }
}