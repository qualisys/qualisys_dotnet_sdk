using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QtmCaptureTimer
{
    public class Help
    {
        public static void ShowHelp()
        {
            MessageBox.Show(
@"How to use it:

Starting without parameters will result in a realtime / capture ticker discovering and using the first available QTM connection.

Two parameters exist and these parameters can be used in any order or not at all.

   -Use 'ip:ipaddress' to specify ipaddress of QTM machine
    (for example ip:127.0.0.1 for localhost)
   -Use 'timecode' to specify that any available timecode
    should be used as display (SMPTE, IRIG, Ptp time).
    If no timecode can be found then the time will stay 00:00:00:00.

So starting program with:
    QTMCaptureTimer ip:127.0.0.1
    will give a timer for localhost QTM

    QTMCaptureTimer timecode
    will give a timecode timer for the first discovered QTM

    QTMCaptureTimer timecode ip:127.0.0.1
    or
    QTMCaptureTimer ip:127.0.0.1 timecode
    will give a timecode timer for localhost QTM", "QtmCaptureTimer Help");
        }
    }
}
