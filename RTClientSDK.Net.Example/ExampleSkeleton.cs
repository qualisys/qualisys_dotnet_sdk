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
                Thread.Sleep(500);
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
                        for (int jointIndex = 0; jointIndex < numberOfSegments; jointIndex++)
                        {
                            var jointSetting = skeletonSetting.Segments[jointIndex];
                            var joint = skeletonData[skeletonIndex].Segments[jointIndex];
                            Console.WriteLine("Frame:{0:D5} Skeleton:{1,16} Segment:{2,16} ID:{3,4} PID:{4,4} X:{5,7:F1} Y:{6,7:F1} Z:{7,7:F1} A:{8,7:F2} B:{9,7:F2} C:{10,7:F2} D:{11,7:F2}",
                                mRtProtocol.GetRTPacket().Frame,
                                skeletonSetting.Name,
                                jointSetting.Name,
                                jointSetting.ID,
                                jointSetting.ParentID,
                                joint.Position.X, joint.Position.Y, joint.Position.Z,
                                joint.Rotation.X, joint.Rotation.Y, joint.Rotation.Z, joint.Rotation.W);
                        }
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