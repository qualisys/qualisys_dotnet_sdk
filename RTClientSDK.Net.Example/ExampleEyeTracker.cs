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
            mRtProtocol.Receive(out packetType, false);

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
                        Console.WriteLine("Eye tracker: {0,-20} Frequency: {1,-5}",
                            eyeTrackerSetting.Name,
                            eyeTrackerSetting.Frequency);
                        var sampleNumber = eyeTracker.SampleNumber;
                        foreach (var sample in eyeTracker.EyeTrackerData )
                        {
                            Console.WriteLine("      SampleNumber:{0,-5} LeftEyeDiameter: {1,-7:F1} RightEyeDiameter: {2,-7:F1}",
                                sampleNumber++, sample.LeftPupilDiameter, sample.RightPupilDiameter);
                        }
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