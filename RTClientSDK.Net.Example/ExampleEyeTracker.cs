// Realtime SDK Example for Qualisys Track Manager. Copyright 2017 Qualisys AB
//
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using System;
using System.Threading;

namespace RTClientSDK.Net.Example
{
    class ExampleEyeTracker : Example
    {
        public ExampleEyeTracker(RTProtocol rtProtocol, string ipAddress) : base(rtProtocol, ipAddress)
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

            // Check for available eye tracker data in the stream
            if (mRtProtocol.EyeTrackerSettings == null)
            {
                if (!mRtProtocol.GetEyeTrackerSettings())
                {
                    Console.WriteLine("QTM: Trying to get Eye tracker settings");
                    Thread.Sleep(500);
                    return;
                }
                Console.WriteLine("QTM: Eye tracker settings available");

                mRtProtocol.StreamAllFrames(QTMRealTimeSDK.Data.ComponentType.ComponentEyeTracker);
                Console.WriteLine("QTM: Starting to stream Eye tracker data");
            }

            // Get RTPacket from stream
            PacketType packetType;
            mRtProtocol.ReceiveRTPacket(out packetType, false);

            // Handle data packet
            if (packetType == PacketType.PacketData)
            {
                var eyeTrackers = mRtProtocol.GetRTPacket().GetEyeTrackerData();
                if (eyeTrackers != null)
                {
                    // Print out the available eye tracker data.
                    for (int eyeTrackerIndex = 0; eyeTrackerIndex < eyeTrackers.Count; eyeTrackerIndex++)
                    {
                        var eyeTracker = eyeTrackers[eyeTrackerIndex];
                        var eyeTrackerSetting = mRtProtocol.EyeTrackerSettings.EyeTrackers[eyeTrackerIndex];
                        Console.WriteLine("Frame:{0:D5} Eye tracker: {1,-20} Frequency: {2,-5} LeftEyeDiameter: {3,-7:F1} RightEyeDiameter: {4,-7:F1} SampleNumber:{5,-5}",
                            mRtProtocol.GetRTPacket().Frame,
                            eyeTrackerSetting.Name,
                            eyeTrackerSetting.Frequency,
                            eyeTracker.LeftPupilDiameter, eyeTracker.RightPupilDiameter, eyeTracker.SampleNumber);
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