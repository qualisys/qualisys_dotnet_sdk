// Realtime SDK Example for Qualisys Track Manager. Copyright 2017 Qualisys AB
//
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using System;
using System.Threading;

namespace RTClientSDK.Net.Example
{
    class ExampleSkeleton : Example
    {
        public ExampleSkeleton(RTProtocol rtProtocol, string ipAddress) : base(rtProtocol, ipAddress)
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

            // Check for available Skeleton data in the stream
            if (mRtProtocol.SkeletonSettings == null)
            {
                if (!mRtProtocol.GetSkeletonSettings())
                {
                    Console.WriteLine("QTM: Trying to get Skeleton settings");
                    Thread.Sleep(500);
                    return;
                }
                Console.WriteLine("QTM: Skeleton settings available");

                mRtProtocol.StreamAllFrames(QTMRealTimeSDK.Data.ComponentType.ComponentSkeleton);
                Console.WriteLine("QTM: Starting to stream Skeleton data");
            }

            // Get RTPacket from stream
            PacketType packetType;
            mRtProtocol.ReceiveRTPacket(out packetType, false);

            // Handle data packet
            if (packetType == PacketType.PacketData)
            {
                var skeletonData = mRtProtocol.GetRTPacket().GetSkeletonData();
                if (skeletonData != null)
                {
                    // Print out the available Skeleton data.
                    for (int skeletonIndex = 0; skeletonIndex < skeletonData.Count; skeletonIndex++)
                    {
                        var skeleton = skeletonData[skeletonIndex];
                        var skeletonSetting = mRtProtocol.SkeletonSettings.Skeletons[skeletonIndex];
                        var numberOfSegments = skeletonData[skeletonIndex].SegmentCount;
                        for (int segmentIndex = 0; segmentIndex < numberOfSegments; segmentIndex++)
                        {
                            var segmentSetting = skeletonSetting.Segments[segmentIndex];
                            var segment = skeletonData[skeletonIndex].Segments[segmentIndex];
                            Console.WriteLine("Frame:{0:D5} Skeleton:{1,16} Segment:{2,16} ID:{3,4} PID:{4,4} X:{5,7:F1} Y:{6,7:F1} Z:{7,7:F1} A:{8,7:F2} B:{9,7:F2} C:{10,7:F2} D:{11,7:F2}",
                                mRtProtocol.GetRTPacket().Frame,
                                skeletonSetting.Name,
                                segmentSetting.Name,
                                segmentSetting.ID,
                                segmentSetting.ParentID,
                                segment.Position.X, segment.Position.Y, segment.Position.Z,
                                segment.Rotation.X, segment.Rotation.Y, segment.Rotation.Z, segment.Rotation.W);
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