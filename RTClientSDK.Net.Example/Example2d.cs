// Realtime SDK Example for Qualisys Track Manager. Copyright 2017 Qualisys AB
//
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using System;
using System.Linq;
using System.Threading;

namespace RTClientSDK.Net.Example
{
    class Example2D : Example
    {
        public Example2D(RTProtocol rtProtocol, string ipAddress) : base(rtProtocol, ipAddress)
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

            if (mRtProtocol.GeneralSettings == null)
            {
                if (!mRtProtocol.GetGeneralSettings())
                {
                    Console.WriteLine("QTM: Trying to get General settings");
                    Thread.Sleep(500);
                    return;
                }
                Console.WriteLine("QTM: General settings available");

                Console.WriteLine("Frequency: {0}", mRtProtocol.GeneralSettings.CaptureFrequency);
                foreach (var camera in mRtProtocol.GeneralSettings.CameraSettings)
                {
                    Console.WriteLine("{0}", camera.ModelAsString);
                }

                mRtProtocol.StreamAllFrames(QTMRealTimeSDK.Data.ComponentType.Component2d);
                Console.WriteLine("QTM: Starting to stream 2d data");
            }

            // Get RTPacket from stream
            PacketType packetType;
            mRtProtocol.Receive(out packetType, false);

            // Handle data packet
            if (packetType == PacketType.PacketData)
            {
                var twoDData = mRtProtocol.GetRTPacket().Get2DMarkerData();
                if (twoDData != null && twoDData.Count() > 0)
                {
                    var twoDForCamera0 = twoDData.First();
                    Console.WriteLine("Frame:{0:D5} Markers:{1} Status:{2}",
                        mRtProtocol.GetRTPacket().Frame,
                        twoDForCamera0.MarkerCount,
                        twoDForCamera0.StatusFlags);
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
