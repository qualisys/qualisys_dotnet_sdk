using System;
using System.Collections.Generic;
using System.Text;
using MccDaq;
using ErrorDefs;

namespace AnalogIO
{
    public class clsAnalogIO
    {
        public const int ANALOGINPUT = 1;
        public const int ANALOGOUTPUT = 2;
        public const int PRETRIGIN = 9;
        public const int ATRIGIN = 17;

        public int ATrigRes;
        public int ATrigRange;
        private MccDaq.MccBoard TestBoard;
        private int ADRes, DARes;
        private MccDaq.Range[] ValidRanges;

        public int FindAnalogChannelsOfType(MccDaq.MccBoard DaqBoard,
            int AnalogType, out int Resolution, out MccDaq.Range DefaultRange,
            out int DefaultChan, out MccDaq.TriggerType DefaultTrig)
        {
            int ChansFound, IOType;
            MccDaq.ErrorInfo ULStat;
            bool CheckPretrig, CheckATrig = false;
            MccDaq.Range TestRange;
            bool RangeFound;

            // check supported features by trial 
            // and error with error handling disabled
            ULStat = MccDaq.MccService.ErrHandling
                (MccDaq.ErrorReporting.DontPrint, MccDaq.ErrorHandling.DontStop);

            TestBoard = DaqBoard;
            ATrigRes = 0;
            DefaultChan = 0;
            DefaultTrig = TriggerType.TrigPosEdge;
            DefaultRange = Range.NotUsed;
            Resolution = 0;
            IOType = (AnalogType & 3);
            switch (IOType)
            {
                case ANALOGINPUT:
                    // Get the number of A/D channels
                    ULStat = DaqBoard.BoardConfig.GetNumAdChans(out ChansFound);
                    if (!(ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors))
                    {
                        clsErrorDefs.DisplayError(ULStat);
                        return ChansFound;
                    }
                    if (ChansFound > 0)
                    {
                        // Get the resolution of A/D
                        ULStat = DaqBoard.BoardConfig.GetAdResolution(out ADRes);
                        if (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors)
                            Resolution = ADRes;
                        // check ranges for a valid default
                        RangeFound = TestInputRanges(out TestRange);
                        if (RangeFound) DefaultRange = TestRange;
                    }
                    break;
                default:
                    // Get the number of D/A channels
                    ULStat = DaqBoard.BoardConfig.GetNumDaChans(out ChansFound);
                    if (!(ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors))
                    {
                        clsErrorDefs.DisplayError(ULStat);
                        return ChansFound;
                    }
                    if (ChansFound > 0)
                    {
                        ULStat = TestBoard.GetConfig(2, 0, 292, out DARes);
                        Resolution = DARes;
                        RangeFound = TestOutputRanges(out TestRange);
                        if (RangeFound) DefaultRange = TestRange;
                    }
                    break;
            }

            CheckATrig = ((AnalogType & ATRIGIN) == ATRIGIN);
            if ((ChansFound > 0) & CheckATrig)
            {
                ULStat = DaqBoard.SetTrigger(MccDaq.TriggerType.TrigAbove, 0, 0);
                if (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors)
                {
                    DefaultTrig = MccDaq.TriggerType.TrigAbove;
                    GetTrigResolution();
                }
                else
                    ChansFound = 0;
            }

            CheckPretrig = ((AnalogType & PRETRIGIN) == PRETRIGIN);
            if ((ChansFound > 0) & CheckPretrig)
            {
                // if DaqSetTrigger supported, trigger type is analog
                ULStat = DaqBoard.DaqSetTrigger(MccDaq.TriggerSource.TrigImmediate,
                    MccDaq.TriggerSensitivity.AboveLevel, 0, MccDaq.ChannelType.Analog,
                    DefaultRange, 0.0F, 0.1F, MccDaq.TriggerEvent.Start);
                if (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors)
                    DefaultTrig = MccDaq.TriggerType.TrigAbove;
                else
                {
                    ULStat = DaqBoard.SetTrigger(MccDaq.TriggerType.TrigPosEdge, 0, 0);
                    if (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors)
                        DefaultTrig = MccDaq.TriggerType.TrigPosEdge;
                    else
                        ChansFound = 0;
                }
            }
            ULStat = MccDaq.MccService.ErrHandling
                (clsErrorDefs.ReportError, clsErrorDefs.HandleError);
            return ChansFound;
        }

        private void GetTrigResolution()
        {
            int BoardID, TrigSource;
            MccDaq.ErrorInfo ULStat;

            ULStat = TestBoard.GetConfig(2, 0, 209, out TrigSource);
            ULStat = TestBoard.BoardConfig.GetBoardType(out BoardID);

            switch (BoardID)
            {
                case 95:
                case 96:
                case 97:
                case 98:
                case 102:
                case 165:
                case 166:
                case 167:
                case 168:
                case 177:
                case 178:
                case 179:
                case 180:
                case 203:
                case 204:
                case 205:
                case 213:
                case 214:
                case 215:
                case 216:
                case 217:
                    {
                        //PCI-DAS6030, 6031, 6032, 6033, 6052
                        //USB-1602HS, 1602HS-2AO, 1604HS, 1604HS-2AO
                        //PCI-2511, 2513, 2515, 2517, USB-2523, 2527, 2533, 2537
                        //USB-1616HS, 1616HS-2, 1616HS-4, 1616HS-BNC
                        ATrigRes = 12;
                        ATrigRange = 20;
                        if (TrigSource > 0) ATrigRange = -1;
                    }
                    break;
                case 101:
                case 103:
                case 104:
                    {
                        //PCI-DAS6040, 6070, 6071
                        ATrigRes = 8;
                        ATrigRange = 20;
                        if (TrigSource > 0) ATrigRange = -1;
                    }
                    break;
                default:
                    {
                        ATrigRes = 0;
                        ATrigRange = -1;
                    }
                    break;
            }
        }

        private bool TestInputRanges(out MccDaq.Range DefaultRange)
        {
            short dataValue;
            int dataHRValue, Options, index;
            MccDaq.ErrorInfo ULStat;
            MccDaq.Range TestRange;
            bool RangeFound = false;
            string ConnectionConflict;

            ConnectionConflict = "This network device is in use by another process or user." +
               System.Environment.NewLine + System.Environment.NewLine +
               "Check for other users on the network and close any applications " +
               System.Environment.NewLine +
               "(such as Instacal) that may be accessing the network device.";

            ValidRanges = new MccDaq.Range[49];
            DefaultRange = MccDaq.Range.NotUsed;
            TestRange = MccDaq.Range.NotUsed;
            Options = 0;
            index = 0;
            foreach (int i in Enum.GetValues(TestRange.GetType()))
            {
                TestRange = (MccDaq.Range) i;
                if (ADRes > 16)
                    ULStat = TestBoard.AIn32(0, TestRange, out dataHRValue, Options);
                else
                    ULStat = TestBoard.AIn(0, TestRange, out dataValue);
                if (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors)
                {
                    if (DefaultRange == MccDaq.Range.NotUsed)
                        DefaultRange = TestRange;
                    ValidRanges.SetValue(TestRange, index);
                    index = index + 1;
                    RangeFound = true;
                }
                else
                {
                    if ((ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NetDevInUseByAnotherProc) 
                        || (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NetDevInUse) )
                    {
                        Console.WriteLine(ConnectionConflict + " Device In Use");
                        break;
                    }
                }
            }
            Array.Resize(ref ValidRanges, index);
            return RangeFound;

        }

        private bool TestOutputRanges(out MccDaq.Range DefaultRange)
        {
            short dataValue = 0;
            MccDaq.ErrorInfo ULStat;
            MccDaq.Range TestRange;
            bool RangeFound = false;
            int configVal;
            string ConnectionConflict;

            ConnectionConflict = "This network device is in use by another process or user." +
               System.Environment.NewLine + System.Environment.NewLine +
               "Check for other users on the network and close any applications " +
               System.Environment.NewLine +
               "(such as Instacal) that may be accessing the network device.";

            DefaultRange = MccDaq.Range.NotUsed;
            TestRange = (MccDaq.Range)(-5);
            ULStat = TestBoard.AOut(0, TestRange, dataValue);
            if (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors)
            {
                ULStat = TestBoard.GetConfig(2, 0, 114, out configVal);
                if (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors)
                {
                    DefaultRange = (MccDaq.Range)configVal;
                    RangeFound = true;
                }
            }
            else
            {
                TestRange = MccDaq.Range.NotUsed;
                foreach (int i in Enum.GetValues(TestRange.GetType()))
                {
                    TestRange = (MccDaq.Range)i;
                    ULStat = TestBoard.AOut(0, TestRange, dataValue);
                    if (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors)
                    {
                        if (DefaultRange == MccDaq.Range.NotUsed)
                            DefaultRange = TestRange;
                        RangeFound = true;
                        break;
                    }
                    else
                    {
                        if ((ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NetDevInUseByAnotherProc)
                            || (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NetDevInUse))
                        {
                            Console.WriteLine(ConnectionConflict + " Device In Use");
                            break;
                        }
                    }
                }
            }
            return RangeFound;

        }

        public MccDaq.Range[] GetRangeList()
        {
            MccDaq.Range DefaultRange;
            MccDaq.ErrorInfo ULStat;

            // check supported ranges by trial 
            // and error with error handling disabled
            ULStat = MccDaq.MccService.ErrHandling
                (MccDaq.ErrorReporting.DontPrint, MccDaq.ErrorHandling.DontStop);

            TestInputRanges(out DefaultRange);

            ULStat = MccDaq.MccService.ErrHandling
                (clsErrorDefs.ReportError, clsErrorDefs.HandleError);
            return ValidRanges;
        }

        public float GetRangeVolts(MccDaq.Range Range)
        {
            string RangeString;
            float RangeVolts;

            GetRangeInfo(Range, out RangeString, out RangeVolts);
            return RangeVolts;
        }

        private void GetRangeInfo(MccDaq.Range Range, out string RangeString, out float RangeVolts)
        {
            switch (Range)
            {
                case MccDaq.Range.NotUsed:
                    RangeString = "NOTUSED";
                    RangeVolts = 0;
                    break;
                case MccDaq.Range.Bip60Volts:
                    RangeString = "BIP60VOLTS";
                    RangeVolts = 120;
                    break;
                case MccDaq.Range.Bip30Volts:
                    RangeString = "BIP30VOLTS";
                    RangeVolts = 60;
                    break;
                case MccDaq.Range.Bip20Volts:
                    RangeString = "BIP20VOLTS";
                    RangeVolts = 40;
                    break;
                case MccDaq.Range.Bip15Volts:
                    RangeString = "BIP15VOLTS";
                    RangeVolts = 30;
                    break;
                case Range.Bip10Volts:
                    RangeString = "BIP10VOLTS";
                    RangeVolts = 20;
                    break;
                case Range.Bip5Volts:
                    RangeString = "BIP5VOLTS";
                    RangeVolts = 10;
                    break;
                case MccDaq.Range.Bip4Volts:
                    RangeString = "BIP4VOLTS";
                    RangeVolts = 8;
                    break;
                case Range.Bip2Pt5Volts:
                    RangeString = "BIP2PT5VOLTS";
                    RangeVolts = 5;
                    break;
                case MccDaq.Range.Bip2Volts:
                    RangeString = "BIP2VOLTS";
                    RangeVolts = 4;
                    break;
                case Range.Bip1Pt25Volts:
                    RangeString = "BIP1PT25VOLTS";
                    RangeVolts = 2.5F;
                    break;
                case Range.Bip1Volts:
                    RangeString = "BIP1VOLTS";
                    RangeVolts = 2;
                    break;
                case Range.BipPt625Volts:
                    RangeString = "BIPPT625VOLTS";
                    RangeVolts = 1.25F;
                    break;
                case Range.BipPt5Volts:
                    RangeString = "BIPPT5VOLTS";
                    RangeVolts = 1;
                    break;
                case Range.BipPt1Volts:
                    RangeString = "BIPPT1VOLTS";
                    RangeVolts = 0.2F;
                    break;
                case Range.BipPt05Volts:
                    RangeString = "BIPPT05VOLTS";
                    RangeVolts = 0.1F;
                    break;
                case MccDaq.Range.BipPt312Volts:
                    RangeString = "BIPPT312VOLTS";
                    RangeVolts = 0.624F;
                    break;
                case MccDaq.Range.BipPt25Volts:
                    RangeString = "BIPPT25VOLTS";
                    RangeVolts = 0.5F;
                    break;
                case MccDaq.Range.BipPt2Volts:
                    RangeString = "BIPPT2VOLTS";
                    RangeVolts = 0.4F;
                    break;
                case MccDaq.Range.BipPt156Volts:
                    RangeString = "BIPPT156VOLTS";
                    RangeVolts = 0.3125F;
                    break;
                case MccDaq.Range.BipPt125Volts:
                    RangeString = "BIPPT125VOLTS";
                    RangeVolts = 0.25F;
                    break;
                case MccDaq.Range.BipPt078Volts:
                    RangeString = "BIPPT078VOLTS";
                    RangeVolts = 0.15625F;
                    break;
                case Range.BipPt01Volts:
                    RangeString = "BIPPT01VOLTS";
                    RangeVolts = 0.02F;
                    break;
                case Range.BipPt005Volts:
                    RangeString = "BIPPT005VOLTS";
                    RangeVolts = 0.01F;
                    break;
                case Range.Bip1Pt67Volts:
                    RangeString = "BIP1PT67VOLTS";
                    RangeVolts = 3.34F;
                    break;
                case Range.Uni10Volts:
                    RangeString = "UNI10VOLTS";
                    RangeVolts = 10;
                    break;
                case Range.Uni5Volts:
                    RangeString = "UNI5VOLTS";
                    RangeVolts = 5;
                    break;
                case MccDaq.Range.Uni4Volts:
                    RangeString = "UNI4VOLTS";
                    RangeVolts = 4.096F;
                    break;
                case Range.Uni2Pt5Volts:
                    RangeString = "UNI2PT5VOLTS";
                    RangeVolts = 2.5F;
                    break;
                case Range.Uni2Volts:
                    RangeString = "UNI2VOLTS";
                    RangeVolts = 2;
                    break;
                case Range.Uni1Pt25Volts:
                    RangeString = "UNI1PT25VOLTS";
                    RangeVolts = 1.25F;
                    break;
                case Range.Uni1Volts:
                    RangeString = "UNI1VOLTS";
                    RangeVolts = 1;
                    break;
                case MccDaq.Range.UniPt25Volts:
                    RangeString = "UNIPT25VOLTS";
                    RangeVolts = 0.25F;
                    break;
                case MccDaq.Range.UniPt2Volts:
                    RangeString = "UNIPT2VOLTS";
                    RangeVolts = 0.2F;
                    break;
                case Range.UniPt1Volts:
                    RangeString = "UNIPT1VOLTS";
                    RangeVolts = 0.1F;
                    break;
                case MccDaq.Range.UniPt05Volts:
                    RangeString = "UNIPT05VOLTS";
                    RangeVolts = 0.05F;
                    break;
                case Range.UniPt01Volts:
                    RangeString = "UNIPT01VOLTS";
                    RangeVolts = 0.01F;
                    break;
                case Range.UniPt02Volts:
                    RangeString = "UNIPT02VOLTS";
                    RangeVolts = 0.02F;
                    break;
                case Range.Uni1Pt67Volts:
                    RangeString = "UNI1PT67VOLTS";
                    RangeVolts = 1.67F;
                    break;
                case Range.Ma4To20:
                    RangeString = "MA4TO20";
                    RangeVolts = 16;
                    break;
                case Range.Ma2To10:
                    RangeString = "MA2to10";
                    RangeVolts = 8;
                    break;
                case Range.Ma1To5:
                    RangeString = "MA1TO5";
                    RangeVolts = 4;
                    break;
                case Range.MaPt5To2Pt5:
                    RangeString = "MAPT5TO2PT5";
                    RangeVolts = 2;
                    break;
                case MccDaq.Range.Ma0To20:
                    RangeString = "MA0TO20";
                    RangeVolts = 20;
                    break;
                case MccDaq.Range.BipPt025Amps:
                    RangeString = "BIPPT025A";
                    RangeVolts = 0.05F;
                    break;
                case MccDaq.Range.BipPt025VoltsPerVolt:
                    RangeString = "BIPPT025VPERV";
                    RangeVolts = 0.05F;
                    break;
                default:
                    RangeString = "NOTUSED";
                    RangeVolts = 0;
                    break;
            }
        }
    }
}
