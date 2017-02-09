using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using QTMRealTimeSDK.Settings;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SixDofViewer
{
    public partial class MainWindow : Form
    {
        RTProtocol rtProtocol = new RTProtocol();
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        // Default to localhost if no QTM is discovered on network
        string ipAddress = "127.0.0.1";
        // Default to first body in project/file if no body name is specified on the command line
        string sixDofBodyNameToUse = "";
        int bodyIndexToUse = 0;
        public EulerNames eulerNames;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Take the first parameter of the command line as ipaddress if any (like 192.168.10.156).
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                ipAddress = args[1];
                if (args.Length > 2)
                {
                    // If a second command line parameter exists then use this as the body to display.
                    // Useful if project/file contains multiple bodies since this program only displays information about one body.
                    sixDofBodyNameToUse = args[2];
                }
            }
            else
            {
                // If no command line parameters was found the try and discover servers on the network
                if (rtProtocol.DiscoverRTServers(4545))
                {
                    // If any found use the ipaddress of the first in the list...
                    var discoveryResponses = rtProtocol.DiscoveryResponses;
                    if (discoveryResponses.Count >= 1)
                    {
                        ipAddress = discoveryResponses.First().IpAddress;
                    }
                }
            }

            // Start a timer with a 50ms tick frequency
            timer.Tick += Timer_Tick;
            timer.Interval = 50;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Try and connect
            if (!rtProtocol.IsConnected())
            {
                if (!rtProtocol.Connect(ipAddress))
                {
                    Color.BackColor = System.Drawing.Color.Red;
                    return;
                }

                Color.BackColor = System.Drawing.Color.OrangeRed;
            }

            // Check for available 6DOF data in the stream
            if (rtProtocol.Settings6DOF == null)
            {
                if (!rtProtocol.Get6dSettings())
                {
                    return;
                }

                Color.BackColor = System.Drawing.Color.Yellow;

                if (sixDofBodyNameToUse.Length > 0)
                {
                    for (int bodyIndex = 0; bodyIndex < rtProtocol.Settings6DOF.bodyCount; bodyIndex++)
                    {
                        if (string.Equals(rtProtocol.Settings6DOF.bodies[bodyIndex].Name, sixDofBodyNameToUse, StringComparison.OrdinalIgnoreCase))
                        {
                            bodyIndexToUse = bodyIndex;
                            break;
                        }
                    }
                }
                else
                {
                    if (rtProtocol.Settings6DOF.bodyCount > 0)
                    {
                        sixDofBodyNameToUse = rtProtocol.Settings6DOF.bodies[0].Name;
                        bodyIndexToUse = 0;
                    }
                }

                eulerNames = rtProtocol.Settings6DOF.eulerNames;

                // Start streaming 6dof euler residual data at 10Hz frequency
                rtProtocol.StreamFrames(StreamRate.RateFrequency, 10, QTMRealTimeSDK.Data.ComponentType.Component6dEulerResidual);
                Thread.Sleep(500);
            }

            // Get RTPacket from stream
            PacketType packetType;
            rtProtocol.ReceiveRTPacket(out packetType, false);

            // Handle data packet
            if (packetType == PacketType.PacketData)
            {
                Color.BackColor = System.Drawing.Color.Green;

                var sixDofData = rtProtocol.GetRTPacket().Get6DOFEulerResidualData();
                if (sixDofData != null)
                {
                    // Put 6dof data information in the labels
                    if (sixDofData.Count > bodyIndexToUse)
                    {
                        var sixDofBody = sixDofData[bodyIndexToUse];

                        this.Body.Text = sixDofBodyNameToUse;
                        this.X.Text = float.IsNaN(sixDofBody.Position.X) ? "X:---" : string.Format("X:{0:F1}", sixDofBody.Position.X);
                        this.Y.Text = float.IsNaN(sixDofBody.Position.Y) ? "Y:---" : string.Format("Y:{0:F1}", sixDofBody.Position.Y);
                        this.Z.Text = float.IsNaN(sixDofBody.Position.Z) ? "Z:---" : string.Format("Z:{0:F1}", sixDofBody.Position.Z);
                        this.Residual.Text = float.IsNaN(sixDofBody.Residual) ? "Residual:---" : string.Format("Residual:{0:F1}", sixDofBody.Residual);
                        this.First.Text = float.IsNaN(sixDofBody.Rotation.First) ? string.Format("{0}:---", eulerNames.First) : string.Format("{0}:{1:F1}", eulerNames.First, sixDofBody.Rotation.First);
                        this.Second.Text = float.IsNaN(sixDofBody.Rotation.Second) ? string.Format("{0}:---", eulerNames.Second) : string.Format("{0}:{1:F1}", eulerNames.Second, sixDofBody.Rotation.Second);
                        this.Third.Text = float.IsNaN(sixDofBody.Rotation.Third) ? string.Format("{0}:---", eulerNames.Third) : string.Format("{0}:{1:F1}", eulerNames.Third, sixDofBody.Rotation.Third);
                    }
                }
            }

            // Handle event packet
            if (packetType == PacketType.PacketEvent)
            {
                var qtmEvent = rtProtocol.GetRTPacket().GetEvent();
                switch (qtmEvent)
                {
                    case QTMEvent.EventConnectionClosed:
                    case QTMEvent.EventCaptureStopped:
                    case QTMEvent.EventCalibrationStopped:
                    case QTMEvent.EventRTFromFileStopped:
                    case QTMEvent.EventQTMShuttingDown:

                        // If QTM is shutting down then handle it, disconnect and empty labels
                        rtProtocol.StreamFramesStop();
                        rtProtocol.Disconnect();

                        Color.BackColor = System.Drawing.Color.Red;

                        this.Body.Text = "Body";
                        this.X.Text = "X";
                        this.Y.Text = "Y";
                        this.Z.Text = "Z";
                        this.Residual.Text = "Residual";
                        this.First.Text = eulerNames.First;
                        this.Second.Text = eulerNames.Second;
                        this.Third.Text = eulerNames.Third;

                        break;
                }
            }
        }
    }
}
