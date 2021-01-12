using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;

namespace QtmCaptureTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //
        // This part below is to be able to replace min and max buttons in the title bar with a help '?' button
        //
        private const uint WS_EX_CONTEXTHELP = 0x00000400;
        private const uint WS_MINIMIZEBOX = 0x00020000;
        private const uint WS_MAXIMIZEBOX = 0x00010000;
        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_FRAMECHANGED = 0x0020;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_CONTEXTHELP = 0xF180;

        [DllImport("user32.dll")]
        private static extern uint GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, uint newStyle);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            uint styles = GetWindowLong(hwnd, GWL_STYLE);
            styles &= 0xFFFFFFFF ^ (WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
            SetWindowLong(hwnd, GWL_STYLE, styles);
            styles = GetWindowLong(hwnd, GWL_EXSTYLE);
            styles |= WS_EX_CONTEXTHELP;
            SetWindowLong(hwnd, GWL_EXSTYLE, styles);
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(HelpHook);
        }

        private IntPtr HelpHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SYSCOMMAND &&
                ((int)wParam & 0xFFF0) == SC_CONTEXTHELP)
            {
                Help.ShowHelp();
                handled = true;
            }
            return IntPtr.Zero;
        }
        //
        //
        //

        private string ipAddress = String.Empty;
        private string hostName = String.Empty;
        private bool useTimecode = false;

        public MainWindow(string ipAddress, bool useTimecode)
        {
            InitializeComponent();

            DataContext = this;

            this.Loaded += MainWindow_Loaded;
            this.ipAddress = ipAddress;
            this.useTimecode = useTimecode;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(ipAddress))
            {
                // If no command line parameters was found the try and discover servers on the network
                using (var rtProtocol = new RTProtocol())
                {
                    if (rtProtocol.DiscoverRTServers(4545))
                    {
                        // If any found use the ipaddress of the first in the list...
                        var discoveryResponses = rtProtocol.DiscoveryResponses;
                        if (discoveryResponses.Count >= 1)
                        {
                            hostName = discoveryResponses.First().HostName;
                            ipAddress = discoveryResponses.First().IpAddress;
                        }
                    }
                }
            }

            var t = new Thread(realtimedata_thread);
            t.Start();

            NotifyPropertyChanged("TitleText");
        }

        private void realtimedata_thread()
        {
            Thread.CurrentThread.IsBackground = true;

            RTProtocol rtProtocol = new RTProtocol();

            while (true)
            {
                try
                {
                    // Try and connect
                    if (!rtProtocol.IsConnected())
                    {
                        if (!rtProtocol.Connect(ipAddress))
                        {
                            Time = "Trying to connect to " + ipAddress;
                            continue;
                        }
                    }

                    // Check for available settings in the stream
                    if (rtProtocol.GeneralSettings == null)
                    {
                        if (!rtProtocol.GetGeneralSettings())
                        {
                            var error = rtProtocol.GetErrorString();
                            if (!String.IsNullOrEmpty(error))
                                Time = error;
                            continue;
                        }

                        rtProtocol.StreamFrames(StreamRate.RateAllFrames, 0, QTMRealTimeSDK.Data.ComponentType.ComponentTimecode);
                    }

                    // Get RTPacket from stream
                    PacketType packetType;
                    if (rtProtocol.Receive(out packetType, false) == ReceiveResponseType.success)
                    {

                        // Handle data packet
                        if (packetType == PacketType.PacketData)
                        {
                            var rtPacket = rtProtocol.GetRTPacket();

                            if (useTimecode)
                            {
                                var timecodes = rtPacket.GetTimecodeData();
                                if (timecodes.Count >= 1)
                                    Time = timecodes[0].FormatTimestamp();
                            }
                            else
                            {
                                var numberOfMicrosSinceStart = rtPacket.TimeStamp;
                                var seconds = (uint)(numberOfMicrosSinceStart / 1000000u);
                                var hour = seconds / 3600;
                                seconds = seconds - hour * 3600;
                                var minute = seconds / 60;
                                var second = seconds - minute * 60;
                                Time = string.Format($"{hour:D2}:{minute:D2}:{second:D2}");
                            }
                        }
                        else if (packetType == PacketType.PacketEvent)
                        {
                            // Handle event packet
                            var qtmEvent = rtProtocol.GetRTPacket().GetEvent();
                            switch (qtmEvent)
                            {
                                case QTMEvent.Connected:
                                case QTMEvent.ConnectionClosed:
                                case QTMEvent.CaptureStarted:
                                case QTMEvent.CaptureStopped:
                                case QTMEvent.CalibrationStarted:
                                case QTMEvent.CalibrationStopped:
                                case QTMEvent.RTFromFileStarted:
                                case QTMEvent.RTFromFileStopped:
                                case QTMEvent.QTMShuttingDown:

                                    // If QTM is shutting down then handle it, disconnect and empty labels
                                    rtProtocol.StreamFramesStop();
                                    rtProtocol.Disconnect();

                                    Time = String.Empty;

                                    break;
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    Time = e.Message;
                }
            }
        }

        public string TitleText
        {
            get
            {
                string ipAddressInTitle = "localhost";
                if (!String.IsNullOrEmpty(ipAddress))
                    ipAddressInTitle = ipAddress;
                string hostInTitle = "";
                if (!String.IsNullOrEmpty(hostName))
                    hostInTitle = hostName + " - ";
                string timecodeInTitle = "";
                if (useTimecode)
                    timecodeInTitle = " - Timecode" ;
                return "QTM Realtime/Capture Timer (" + hostInTitle + ipAddressInTitle + ")" + timecodeInTitle;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        private string time = String.Empty;
        public string Time
        {
            get
            {
                if (String.IsNullOrEmpty(time))
                    return useTimecode ? "00:00:00:00" : "00:00:00";
                return time;
            }
            set
            {
                if (time != value)
                {
                    time = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
