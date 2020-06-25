// Realtime SDK Example for Qualisys Track Manager. Copyright 2016-2017 Qualisys AB
//
using System;
using System.Collections.Generic;
using System.Threading;
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;

namespace RTClientSDK.Net.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            RTProtocol mRtProtocol = new RTProtocol();
            string mIpAddress = "127.0.0.1";

            //Example example = new ExampleSkeleton(mRtProtocol, mIpAddress);
            //Example example = new Example3D(mRtProtocol, mIpAddress);
            //Example example = new ExampleImage(mRtProtocol, mIpAddress);
            //Example example = new Example2D(mRtProtocol, mIpAddress);
            //Example example = new Example6D(mRtProtocol, mIpAddress);
            //Example example = new ExampleGaze(mRtProtocol, mIpAddress);
            //Example example = new ExampleEyeTracker(mRtProtocol, mIpAddress);
            //Example example = new ExampleTimecode(mRtProtocol, mIpAddress);
            Example example = new ExampleUDP(mRtProtocol, mIpAddress);
            MainExample mainExample = new MainExample(example, mRtProtocol, mIpAddress);
            mainExample.DiscoverQTMServers(4545);
            Console.WriteLine("Press key to continue");
            Console.ReadKey();
            while (true)
            {
                mainExample.Run();
                if (Console.KeyAvailable)
                {
                    if (Console.ReadKey(false).Key == ConsoleKey.Escape)
                        break;
                }
            }
        }
    }

    class MainExample
    {
        private string mIpAddress;
        private Example mExample;
        private RTProtocol mRtProtocol;

        public MainExample(Example example, RTProtocol rtProtocol, string ipAddress)
        {
            mIpAddress = ipAddress;
            mExample = example;
            mRtProtocol = rtProtocol;
        }

        public void DiscoverQTMServers(ushort discoveryPort)
        {
            if (mRtProtocol.DiscoverRTServers(discoveryPort))
            {
                var discoveryResponses = mRtProtocol.DiscoveryResponses;
                foreach (var discoveryResponse in discoveryResponses)
                {
                    Console.WriteLine("Host:{0,20}\tIP Adress:{1,15}\tInfo Text:{2,20}\tCamera count:{3,3}", discoveryResponse.HostName, discoveryResponse.IpAddress, discoveryResponse.InfoText, discoveryResponse.CameraCount);
                }
            }
        }
        public void Run()
        {
            mExample.HandleStreaming();
        }
    }
}