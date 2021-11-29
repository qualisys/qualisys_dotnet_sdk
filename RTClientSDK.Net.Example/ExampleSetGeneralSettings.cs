using System;
using QTMRealTimeSDK;
using QTMRealTimeSDK.Settings;

namespace RTClientSDK.Net.Example
{
    class ExampleSetGeneralSettings : Example
    {
        public ExampleSetGeneralSettings(RTProtocol rtProtocol, string ipAddress) : base(rtProtocol, ipAddress)
        {         
            if(!rtProtocol.Connect(ipAddress))
            {
                Console.WriteLine("Failed to connect: " + rtProtocol.GetErrorString());
                return;
            }

            if(!rtProtocol.TakeControl("password"))
            {
                Console.WriteLine("Failed to take control: " + rtProtocol.GetErrorString());
                return;
            }

            if(!rtProtocol.GetGeneralSettings())
            {
                Console.WriteLine("Failed to get general settings: " + rtProtocol.GetErrorString());
                return;
            }

            if(!rtProtocol.SetGeneralSettings(rtProtocol.GeneralSettings))
            {
                Console.WriteLine("Failed to set general settings: " + rtProtocol.GetErrorString());
                return;
            }
            
           Console.WriteLine("Ok!");
        }

        public override void HandleStreaming()
        {
        }
    }
}
