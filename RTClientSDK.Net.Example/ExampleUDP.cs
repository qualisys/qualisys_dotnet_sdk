// Realtime SDK Example for Qualisys Track Manager. Copyright 2017 Qualisys AB
//
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using System;
using System.Threading;

namespace RTClientSDK.Net.Example
{
    class ExampleUDP : Example
    {
        public ExampleUDP(RTProtocol rtProtocol, string ipAddress) : base(rtProtocol, ipAddress)
        {
        }

        private string ipAddressToSendUdpPacketsTo = "192.168.10.45";

        public override void HandleStreaming()
        {
            if (!mRtProtocol.IsConnected())
            {
                if (!mRtProtocol.Connect(mIpAddress))
                {
                    Console.WriteLine("QTM: Trying to connect");
                    Thread.Sleep(1000);
                    return;
                }
                Console.WriteLine("QTM: Connected");
            }

            // Check for available 6DOF data in the stream
            if (mRtProtocol.Settings6DOF == null)
            {
                if (!mRtProtocol.Get6dSettings())
                {
                    Console.WriteLine("QTM: Trying to get 6DOF settings");
                    Thread.Sleep(500);
                    return;
                }
                Console.WriteLine("QTM: 6DOF settings available");

                mRtProtocol.StreamAllFrames(QTMRealTimeSDK.Data.ComponentType.Component6dEulerResidual, 3456, ipAddressToSendUdpPacketsTo);
                Console.WriteLine("QTM: Starting to stream 6DOF data");
            }

            // Get RTPacket from stream
            PacketType packetType;
            mRtProtocol.ReceiveRTPacket(out packetType, false);

            // Handle data packet
            if (packetType == PacketType.PacketData)
            {
                // NOTE: No data will arrive here, since we say that the data should be sent using udp to another computer.
            }

            // Handle event packet
            if (packetType == PacketType.PacketEvent)
            {
                // If an event comes from QTM then print it out
                var qtmEvent = mRtProtocol.GetRTPacket().GetEvent();
                if (qtmEvent == QTMEvent.EventConnectionClosed ||
                    qtmEvent == QTMEvent.EventRTFromFileStopped)
                {
                    mRtProtocol.ClearSettings();
                }
                Console.WriteLine("{0}", qtmEvent);
            }
        }
    }
}