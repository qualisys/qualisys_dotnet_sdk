// Realtime SDK Example for Qualisys Track Manager. Copyright 2017 Qualisys AB
//
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using QTMRealTimeSDK.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace RTClientSDK.Net.Example
{
    class ExampleImage : Example
    {
        public ExampleImage(RTProtocol rtProtocol, string ipAddress) : base(rtProtocol, ipAddress)
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
                    Console.WriteLine("{0}", camera.Model);
                }

                if (!mRtProtocol.GetImageSettings())
                {
                    Console.WriteLine("QTM: Trying to get Image settings");
                    Thread.Sleep(500);
                    return;
                }

                Console.WriteLine("QTM: Image settings available");
                Console.WriteLine(mRtProtocol.ImageSettings.Xml);


                mRtProtocol.TakeControl("realtimestreamingpassword");

                SettingsImage newImageSettings = new SettingsImage();
                List<ImageCamera> newImageSettingsCameras = new List<ImageCamera>();
                for (int i = 0; i < mRtProtocol.ImageSettings.Cameras.Count; i++)
                {
                    var camera = mRtProtocol.ImageSettings.Cameras[i];
                    camera.Enabled = true;
                    camera.Width /= 4;
                    camera.Height /= 4;
                    newImageSettingsCameras.Add(camera);
                }
                newImageSettings.Cameras = newImageSettingsCameras;

                XmlSerializer serializer = new XmlSerializer(typeof(QTMRealTimeSDK.Settings.SettingsImage));
                StringBuilder builder = new StringBuilder();
                XmlWriterSettings writerSettings = new XmlWriterSettings();
                writerSettings.OmitXmlDeclaration = true;
                XmlWriter writer = XmlWriter.Create(builder, writerSettings);

                serializer.Serialize(writer, newImageSettings);
                var xmlsettings = "<QTM_Settings>";
                var xmldata = builder.ToString();
                xmlsettings += xmldata;
                xmlsettings += "</QTM_Settings>";

                string response;
                mRtProtocol.SendXML(xmlsettings, out response);
                Console.WriteLine(response);

                mRtProtocol.StreamAllFrames(QTMRealTimeSDK.Data.ComponentType.ComponentImage);
                Console.WriteLine("QTM: Starting to stream Image data");
                Thread.Sleep(500);
            }

            // Get RTPacket from stream
            PacketType packetType;
            mRtProtocol.ReceiveRTPacket(out packetType, false);

            // Handle data packet
            if (packetType == PacketType.PacketData)
            {
                var imageData = mRtProtocol.GetRTPacket().GetImageData();
                if (imageData != null && imageData.Count() > 0)
                {
                    foreach (var imageFromCamera in imageData)
                    {
                        Console.WriteLine("Frame:{0:D5} Camera Index:{1:D3} Width:{2:D5} Height:{3:D5}",
                            mRtProtocol.GetRTPacket().Frame, imageFromCamera.CameraID, imageFromCamera.Width, imageFromCamera.Height);
                    }
                }
            }

            // Handle event packet
            if (packetType == PacketType.PacketEvent)
            {
                // If an event comes from QTM then print it out
                var qtmEvent = mRtProtocol.GetRTPacket().GetEvent();
                Console.WriteLine("{0}", qtmEvent);
            }
        }
    }
}
