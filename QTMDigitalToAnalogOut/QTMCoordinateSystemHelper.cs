using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QTMRealTimeSDK;
using OpenTK;
using QualisysRealTime.Unity.Skeleton;

namespace Jack
{
    class QTMCoordinateSystemHelper
    {

    }

    public static class Rotation
    {
        public enum ECoordinateAxes
        {
            AxisXpositive = 1,
            AxisXnegative = -1,
            AxisYpositive = 2,
            AxisYnegative = -2,
            AxisZpositive = 3,
            AxisZnegative = -3
        };

        public static void GetCalibrationAxesOrder(Axis axisUp, out ECoordinateAxes reNewX, out ECoordinateAxes reNewY, out ECoordinateAxes reNewZ)
        {
            switch (axisUp)
            {
                case Axis.XAxisUpwards:
                    reNewX = ECoordinateAxes.AxisZpositive;
                    reNewY = ECoordinateAxes.AxisXpositive;
                    reNewZ = ECoordinateAxes.AxisYpositive;
                    break;
                case Axis.YAxisUpwards:
                    reNewX = ECoordinateAxes.AxisXpositive;
                    reNewY = ECoordinateAxes.AxisYpositive;
                    reNewZ = ECoordinateAxes.AxisZpositive;
                    break;
                case Axis.ZAxisUpwards:
                    reNewX = ECoordinateAxes.AxisYpositive;
                    reNewY = ECoordinateAxes.AxisZpositive;
                    reNewZ = ECoordinateAxes.AxisXpositive;
                    break;
                case Axis.XAxisDownwards:
                    reNewX = ECoordinateAxes.AxisZnegative;
                    reNewY = ECoordinateAxes.AxisXnegative;
                    reNewZ = ECoordinateAxes.AxisYnegative;
                    break;
                case Axis.YAxisDownwards:
                    reNewX = ECoordinateAxes.AxisXnegative;
                    reNewY = ECoordinateAxes.AxisYnegative;
                    reNewZ = ECoordinateAxes.AxisZnegative;
                    break;
                case Axis.ZAxisDownwards:
                    reNewX = ECoordinateAxes.AxisYnegative;
                    reNewY = ECoordinateAxes.AxisZnegative;
                    reNewZ = ECoordinateAxes.AxisXnegative;
                    break;
                default:
                    reNewX = ECoordinateAxes.AxisXpositive;
                    reNewY = ECoordinateAxes.AxisYpositive;
                    reNewZ = ECoordinateAxes.AxisZpositive;
                    break;
            }
        }


        public static Quaternion GetAxesOrderRotation(ECoordinateAxes eNewX, ECoordinateAxes eNewY, ECoordinateAxes eNewZ)
        {
            Quaternion oRotation = Quaternion.Identity;

            switch (eNewY)
            {
                case ECoordinateAxes.AxisXpositive:
                    {
                        oRotation *= QuaternionHelper2.RotationZ(90 * Mathf.PI / 180);
                        switch (eNewX)
                        {
                            case ECoordinateAxes.AxisYpositive:
                                {
                                    oRotation *= QuaternionHelper2.RotationX(180 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisZpositive:
                                {
                                    oRotation *= QuaternionHelper2.RotationX(90 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisZnegative:
                                {
                                    oRotation *= QuaternionHelper2.RotationX(-90 * Mathf.PI / 180);
                                    break;
                                }
                        }
                        break;
                    }
                case ECoordinateAxes.AxisXnegative:
                    {
                        oRotation *= QuaternionHelper2.RotationZ(-90 * Mathf.PI / 180);
                        switch (eNewX)
                        {
                            case ECoordinateAxes.AxisYnegative:
                                {
                                    oRotation *= QuaternionHelper2.RotationX(180 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisZpositive:
                                {
                                    oRotation *= QuaternionHelper2.RotationX(-90 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisZnegative:
                                {
                                    oRotation *= QuaternionHelper2.RotationX(90 * Mathf.PI / 180);
                                    break;
                                }
                        }
                        break;
                    }
                case ECoordinateAxes.AxisYpositive:
                    {
                        // Retain Y axis.
                        switch (eNewX)
                        {
                            case ECoordinateAxes.AxisXnegative:
                                {
                                    oRotation *= QuaternionHelper2.RotationY(180 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisZpositive:
                                {
                                    oRotation *= QuaternionHelper2.RotationY(90 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisZnegative:
                                {
                                    oRotation *= QuaternionHelper2.RotationY(-90 * Mathf.PI / 180);
                                    break;
                                }
                        }
                        break;
                    }
                case ECoordinateAxes.AxisYnegative:
                    {
                        oRotation *= QuaternionHelper2.RotationX(180 * Mathf.PI / 180);
                        switch (eNewX)
                        {
                            case ECoordinateAxes.AxisXnegative:
                                {
                                    oRotation *= QuaternionHelper2.RotationY(180 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisZpositive:
                                {
                                    oRotation *= QuaternionHelper2.RotationY(90 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisZnegative:
                                {
                                    oRotation *= QuaternionHelper2.RotationY(-90 * Mathf.PI / 180);
                                    break;
                                }
                        }
                        break;
                    }
                case ECoordinateAxes.AxisZpositive:
                    {
                        oRotation *= QuaternionHelper2.RotationX(-90 * Mathf.PI / 180);
                        switch (eNewX)
                        {
                            case ECoordinateAxes.AxisXnegative:
                                {
                                    oRotation *= QuaternionHelper2.RotationZ(180 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisYpositive:
                                {
                                    oRotation *= QuaternionHelper2.RotationZ(-90 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisYnegative:
                                {
                                    oRotation *= QuaternionHelper2.RotationZ(90 * Mathf.PI / 180);
                                    break;
                                }
                        }
                        break;
                    }
                case ECoordinateAxes.AxisZnegative:
                    {
                        oRotation *= QuaternionHelper2.RotationX(90 * Mathf.PI / 180);
                        switch (eNewX)
                        {
                            case ECoordinateAxes.AxisXnegative:
                                {
                                    oRotation *= QuaternionHelper2.RotationZ(180 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisYpositive:
                                {
                                    oRotation *= QuaternionHelper2.RotationZ(180 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisYnegative:
                                {
                                    oRotation *= QuaternionHelper2.RotationZ(180 * Mathf.PI / 180);
                                    break;
                                }
                        }
                        break;
                    }
            }

            //oRotation.normalize();
            return oRotation;
        }
    }
}
