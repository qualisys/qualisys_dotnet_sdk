// Realtime SDK Example for Qualisys Track Manager. Copyright 2017 Qualisys AB
//
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using System;
using System.Threading;

namespace RTClientSDK.Net.Example
{
    class Example6D : Example
    {
        public Example6D(RTProtocol rtProtocol, string ipAddress) : base(rtProtocol, ipAddress)
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

                mRtProtocol.StreamAllFrames(QTMRealTimeSDK.Data.ComponentType.Component6dEulerResidual);
                Console.WriteLine("QTM: Starting to stream 6DOF data");
            }

            // Get RTPacket from stream
            PacketType packetType;
            mRtProtocol.ReceiveRTPacket(out packetType, false);

            // Handle data packet
            if (packetType == PacketType.PacketData)
            {
                var sixDofData = mRtProtocol.GetRTPacket().Get6DOFEulerResidualData();
                if (sixDofData != null)
                {
                    // Print out the available 6DOF data.
                    for (int body = 0; body < sixDofData.Count; body++)
                    {
                        var sixDofBody = sixDofData[body];
                        var bodySetting = mRtProtocol.Settings6DOF.Bodies[body];
                        Console.WriteLine("Frame:{0:D5} Body:{1,20} X:{2,7:F1} Y:{3,7:F1} Z:{4,7:F1} First Angle:{5,7:F1} Second Angle:{6,7:F1} Third Angle:{7,7:F1} Residual:{8,7:F1}",
                            mRtProtocol.GetRTPacket().Frame,
                            bodySetting.Name,
                            sixDofBody.Position.X, sixDofBody.Position.Y, sixDofBody.Position.Z,
                            sixDofBody.Rotation.First, sixDofBody.Rotation.Second, sixDofBody.Rotation.Third,
                            sixDofBody.Residual);
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