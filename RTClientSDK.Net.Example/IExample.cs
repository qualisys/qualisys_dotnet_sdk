// Realtime SDK Example for Qualisys Track Manager. Copyright 2017 Qualisys AB
//
using System;
using QTMRealTimeSDK;

namespace RTClientSDK.Net.Example
{
    internal interface IExample
    {
        void HandleStreaming();
    }

    public abstract class Example : IExample
    {
        public RTProtocol mRtProtocol;
        public string mIpAddress;

        public Example(RTProtocol rtProtocol, string ipAddress)
        {
            mRtProtocol = rtProtocol;
            mIpAddress = ipAddress;
        }
        ~Example()
        {
            if (mRtProtocol.IsConnected())
            {
                mRtProtocol.StreamFramesStop();
                mRtProtocol.Disconnect();
            }
        }

        abstract public void HandleStreaming();
    }
}