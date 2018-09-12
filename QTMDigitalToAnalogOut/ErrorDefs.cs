using System;
using System.Collections.Generic;
using System.Text;
using MccDaq;

namespace ErrorDefs
{

    public class clsErrorDefs
    {
        public static MccDaq.ErrorReporting ReportError;
        public static MccDaq.ErrorHandling HandleError;

        public static void DisplayError(MccDaq.ErrorInfo ErrCode)
        {
            Console.WriteLine("Unexpected Universal Library Error. Error reported: " + ErrCode.Message);
        }

    }

}
