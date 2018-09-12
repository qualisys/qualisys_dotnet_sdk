using System;
using System.Collections.Generic;
using System.Text;
using MccDaq;
using ErrorDefs;

namespace DigitalIO
{
    public class clsDigitalIO
    {
        public const int PORTOUT = 1;
        public const int PORTIN = 2;
        public const int PORTOUTSCAN = 5;
        public const int PORTINSCAN = 10;
        public const int BITOUT = 17;
        public const int BITIN = 34;
        public const int FIXEDPORT = 0;
        public const int PROGPORT = 1;
        public const int PROGBIT = 2;

        public int FindPortsOfType(MccDaq.MccBoard DaqBoard, int PortType,
            out int ProgAbility, out MccDaq.DigitalPortType DefaultPort, 
            out int DefaultNumBits, out int FirstBit)

        {
            int ThisType, NumPorts, NumBits;
            int DefaultDev, InMask, OutMask;
            int PortsFound, curCount, curIndex;
            short status;
            bool PortIsCompatible;
            bool CheckBitProg = false;
            MccDaq.DigitalPortType CurPort;
            MccDaq.FunctionType DFunction;
            MccDaq.ErrorInfo ULStat;
            string ConnectionConflict;

            ULStat = MccDaq.MccService.ErrHandling
                (MccDaq.ErrorReporting.DontPrint, MccDaq.ErrorHandling.DontStop);

            ConnectionConflict = "This network device is in use by another process or user." +
               System.Environment.NewLine + System.Environment.NewLine + 
               "Check for other users on the network and close any applications " +
               System.Environment.NewLine + 
               "(such as Instacal) that may be accessing the network device.";

            DefaultPort = (MccDaq.DigitalPortType)(-1);
            CurPort = DefaultPort;
            PortsFound = 0;
            FirstBit = 0;
            ProgAbility = -1;
            DefaultNumBits = 0;
            ULStat = DaqBoard.BoardConfig.GetDiNumDevs(out NumPorts);
            if (!(ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors))
            {
                clsErrorDefs.DisplayError(ULStat);
                return PortsFound;
            }

            if ((PortType == BITOUT) || (PortType == BITIN))
                CheckBitProg = true;
            if ((PortType == PORTOUTSCAN) || (PortType == PORTINSCAN))
            {
                if (NumPorts > 0)
                {
                    DFunction = MccDaq.FunctionType.DiFunction;
                    if (PortType == PORTOUTSCAN)
                        DFunction = MccDaq.FunctionType.DoFunction;
                    ULStat = DaqBoard.GetStatus(out status, out curCount, out curIndex, DFunction);
                    if (! (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors))
                        NumPorts = 0;
                }
                PortType = PortType & (PORTOUT | PORTIN);
            }

            for (int DioDev = 0; DioDev < NumPorts; ++DioDev)
            {
                ProgAbility = -1;
                ULStat = DaqBoard.DioConfig.GetDInMask(DioDev, out InMask);
                ULStat = DaqBoard.DioConfig.GetDOutMask(DioDev, out OutMask);
                if ((InMask & OutMask) > 0)
                    ProgAbility = FIXEDPORT;
                ULStat = DaqBoard.DioConfig.GetDevType(DioDev, out ThisType);
                if (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors)
                    CurPort = (DigitalPortType)Enum.Parse(typeof(DigitalPortType),
                        ThisType.ToString());
                if ((DioDev == 0) && (CurPort == MccDaq.DigitalPortType.FirstPortCL))
                    //a few devices (USB-SSR08 for example)
                    //start at FIRSTPORTCL and number the bits
                    //as if FIRSTPORTA and FIRSTPORTB exist for
                    //compatibiliry with older digital peripherals
                    FirstBit = 16;

                //check if port is set for requested direction 
                //or can be programmed for requested direction
                PortIsCompatible = false;
                switch (PortType)
                {
                    case (PORTOUT):
                        if (OutMask > 0)
                            PortIsCompatible = true;
                        break;
                    case (PORTIN):
                        if (InMask > 0)
                            PortIsCompatible = true;
                        break;
                    default:
                        PortIsCompatible = false;
                        break;
                }
                PortType = (PortType & (PORTOUT | PORTIN));
                if (!PortIsCompatible)
                {
                    if (ProgAbility != FIXEDPORT)
                    {
                        MccDaq.DigitalPortDirection ConfigDirection;
                        ConfigDirection = DigitalPortDirection.DigitalOut;
                        if (PortType == PORTIN)
                            ConfigDirection = DigitalPortDirection.DigitalIn;
                        if ((CurPort == MccDaq.DigitalPortType.AuxPort) && CheckBitProg)
                        {
                            //if it's an AuxPort, check bit programmability
                            ULStat = DaqBoard.DConfigBit(MccDaq.DigitalPortType.AuxPort,
                                FirstBit, ConfigDirection);
                            if (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors)
                            {
                                //return port to input mode
                                ULStat = DaqBoard.DConfigBit(MccDaq.DigitalPortType.AuxPort,
                                    FirstBit, DigitalPortDirection.DigitalIn); 
                                ProgAbility = PROGBIT;
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
                        if (ProgAbility == -1)
                        {
                            //check port programmability
                            ULStat = DaqBoard.DConfigPort(CurPort, ConfigDirection);
                            if (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors)
                            {
                                //return port to input mode
                                ULStat = DaqBoard.DConfigBit(MccDaq.DigitalPortType.AuxPort,
                                    FirstBit, DigitalPortDirection.DigitalIn);
                                ProgAbility = PROGPORT;
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
                    PortIsCompatible = !(ProgAbility == -1);
                }
                
                if (PortIsCompatible)
                    PortsFound = PortsFound + 1;
                int BitVals, BitWeight;
                int TotalVal, CurBit;
                if (DefaultPort == (MccDaq.DigitalPortType) (- 1))
                {
                    ULStat = DaqBoard.DioConfig.GetNumBits(DioDev, out NumBits);
                    if (ProgAbility == FIXEDPORT)
                    {
                        //could have different number of input and output bits
                        CurBit = 0;
                        TotalVal = 0;
                        BitVals = OutMask;
                        if (PortType == PORTIN) BitVals = InMask;
                        do
                        {
                            BitWeight = (int)Math.Pow(2, CurBit);
                            TotalVal = BitWeight + TotalVal;
                            CurBit = CurBit + 1;
                        } while (TotalVal < BitVals);
                        NumBits = CurBit;
                    }
                    DefaultNumBits = NumBits;
                    DefaultDev = DioDev;
                    DefaultPort = CurPort;
                }
                if (ProgAbility == PROGBIT) break;
            }
            ULStat = MccDaq.MccService.ErrHandling
                (clsErrorDefs.ReportError, clsErrorDefs.HandleError);
            return PortsFound;
        }   
        
    }
}
