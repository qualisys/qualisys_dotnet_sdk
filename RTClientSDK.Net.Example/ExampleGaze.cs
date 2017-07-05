// Realtime SDK Example for Qualisys Track Manager. Copyright 2017 Qualisys AB
//
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using System;
using System.Threading;

namespace RTClientSDK.Net.Example
{
    class ExampleGaze : Example
    {
        public ExampleGaze(RTProtocol rtProtocol, string ipAddress) : base(rtProtocol, ipAddress)
        {
        }

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

            // Check for available Gaze data in the stream
            if (mRtProtocol.GazeVectorSettings == null)
            {
                if (!mRtProtocol.GetGazeVectorSettings())
                {
                    Console.WriteLine("QTM: Trying to get Gaze vector settings");
                    Thread.Sleep(500);
                    return;
                }
                Console.WriteLine("QTM: Gaze vector settings available");

                mRtProtocol.StreamAllFrames(QTMRealTimeSDK.Data.ComponentType.ComponentGazeVector);
                Console.WriteLine("QTM: Starting to stream Gaze vector data");
                Thread.Sleep(500);
            }

            // Get RTPacket from stream
            PacketType packetType;
            mRtProtocol.ReceiveRTPacket(out packetType, false);

            // Handle data packet
            if (packetType == PacketType.PacketData)
            {
                var gazeVectors = mRtProtocol.GetRTPacket().GetGazeVectorData();
                if (gazeVectors != null)
                {
                    // Print out the available Gaze data.
                    for (int gazeVectorIndex = 0; gazeVectorIndex < gazeVectors.Count; gazeVectorIndex++)
                    {
                        var gazeVector = gazeVectors[gazeVectorIndex];
                        var gazeVectorSetting = mRtProtocol.GazeVectorSettings.GazeVectors[gazeVectorIndex];
                        Console.WriteLine("Frame:{0:D5} Gaze vector:{1,20} Frequency:{2,5} PosX:{3,7:F1} PosY:{4,7:F1} PosZ:{5,7:F1} GazeX:{6,7:F1} GazeY:{7,7:F1} GazeZ:{8,7:F1} SampleNumber:{9,5}",
                            mRtProtocol.GetRTPacket().Frame,
                            gazeVectorSetting.Name,
                            gazeVectorSetting.Frequency,
                            gazeVector.Position.X, gazeVector.Position.Y, gazeVector.Position.Z,
                            gazeVector.Gaze.X, gazeVector.Gaze.Y, gazeVector.Gaze.Z,
                            gazeVector.SampleNumber);
                    }
                }
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