// Realtime SDK Example for Qualisys Track Manager. Copyright 2017 Qualisys AB
//
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using System;
using System.Threading;

namespace RTClientSDK.Net.Example
{
    class ExampleTimecode : Example
    {
        public ExampleTimecode(RTProtocol rtProtocol, string ipAddress) : base(rtProtocol, ipAddress)
        {
        }

        public override void HandleStreaming()
        {
            // Check if connection to QTM is possible
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

            // Check for available 3DOF with residual data in the stream
            if (mRtProtocol.GeneralSettings == null)
            {
                if (!mRtProtocol.GetGeneralSettings())
                {
                    Console.WriteLine("QTM: Trying to get general settings");
                    Thread.Sleep(500);
                    return;
                }
                Console.WriteLine("QTM: General settings available");

                mRtProtocol.StreamAllFrames(QTMRealTimeSDK.Data.ComponentType.ComponentTimecode);
                Console.WriteLine("QTM: Starting to stream timecode data");
            }

            // Get RTPacket from stream
            PacketType packetType;
            mRtProtocol.Receive(out packetType, false);

            // Handle data packet
            if (packetType == PacketType.PacketData)
            {
                var packet = mRtProtocol.GetRTPacket();
                var timecodeData = packet.GetTimecodeData();
                if (timecodeData != null)
                {
                    foreach (var timecode in timecodeData)
                    {
                        Console.WriteLine("Frame: {0:D5} Type: {1} Timestamp: {2}", packet.Frame, timecode.Type.ToString(), timecode.ToString());
                    }
                }
            }

            // Handle event packet
            if (packetType == PacketType.PacketEvent)
            {
                // If an event comes from QTM then print it out
                var qtmEvent = mRtProtocol.GetRTPacket().GetEvent();
                if (qtmEvent == QTMEvent.ConnectionClosed ||
                    qtmEvent == QTMEvent.RTFromFileStopped)
                {
                    mRtProtocol.ClearSettings();
                }
                Console.WriteLine("{0}", qtmEvent);
            }
        }
    }
}