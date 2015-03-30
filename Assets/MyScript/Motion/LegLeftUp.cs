using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NUI.Motion
{
    using JointType = Kinect.NuiSkeletonPositionIndex;

    /// <summary>
    /// 左抬腿
    /// </summary>
    partial class LegLeftUp : MotionSuper
    {
        private const float LegRadianThreshold = 0.64f; //弧度阈值；40度约为0.64左右,45度约为0.707
        protected override int Recognise(FeatureData data)
        {
            switch (_motionState)
            {
                case MotionState.Initial:
                    // 判断是否抬高脚
                    if (data.CompareThresholdY(JointType.AnkleLeft, JointType.AnkleRight, 10) == 1) // 用Y判断脚抬高度,脚离地阈值为10
                    {
                        _motionState = MotionState.Valid;
                    }
                    break;
                case MotionState.Valid:
                    // 区别抬脚动作
                    if (data.CompareThresholdZ(JointType.AnkleLeft, JointType.HipLeft, 30) == 0 &&
                        data.CompareThresholdX(JointType.AnkleLeft, JointType.HipLeft, 15) == 0)
                    {
                        _motionState = MotionState.Invalid;
                        return 0;
                    }
                    // 大概的腿长，从踝开始计算
                    float legLength = data.CalculateDifferY(JointType.HipRight, JointType.AnkleRight);
                    if (legLength <= 0) // 腿长不适合理论数据
                    {
                        return 0;
                    }
                    // 在x和z轴上的映射投影距离差值
                    float ankleDifferX = Math.Abs(data.CalculateDifferX(JointType.AnkleLeft, JointType.AnkleRight));
                    float ankleDifferZ = Math.Abs(data.CalculateDifferZ(JointType.AnkleLeft, JointType.AnkleRight));

                    if (ankleDifferX >= ankleDifferZ) // 排除向前或后踢的情况
                    {
                        // 计算腿抬起后裸相差的水平距离
                        float ankleHorizontalDistance = (float)Math.Sqrt(ankleDifferX * ankleDifferX + ankleDifferZ * ankleDifferZ);

                        // 计算腿抬起的弧度
                        float legRadian = ankleHorizontalDistance / legLength;

                        // 当抬起角度超过弧度阈值后生效
                        if (legRadian > LegRadianThreshold)
                        {
                            _motionState = MotionState.Invalid;
                            return 1;
                        }
                    }
                    else
                    {
                        _motionState = MotionState.Invalid;
                    }
                    break;
                default:
                    // 放下脚恢复初始状态
                    if (data.CompareThresholdY(JointType.AnkleLeft, JointType.AnkleRight, 5) == 0)
                    {
                        _motionState = MotionState.Initial;
                    }
                    break;
            }
            return 0;
        }
        protected override MotionType SendCommand(int option)
        {
            return MotionType.LegLeftUp;
        }
    }
}

