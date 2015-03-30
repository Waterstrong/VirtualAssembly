using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NUI.Motion
{
    using JointType = Kinect.NuiSkeletonPositionIndex;

    /// <summary>
    /// 手左向下
    /// </summary>
    partial class HandLeftDown : MotionSuper
    {
        private const double TimeMillisecondsThreshold = 750d; // 时间阈值，以毫秒计算
        protected override int Recognise(FeatureData data)
        {
            // 如果另一只手不在初始位置，则不识别
            if (data.CompareThresholdY(JointType.HandRight, JointType.Spine, 0) == 1)
            {
                _motionState = MotionState.Invalid;
                return 0;
            }
            switch (_motionState)
            {
                case MotionState.Initial:
                    // 如果手超过肩位置，则手抬起开始生效
                    if (data.CompareThresholdY(JointType.HandLeft, JointType.ShoulderLeft, 0) == 1)
                    {
                        _dtBegin = DateTime.Now; // 获取开始时间
                        _motionState = MotionState.Valid;
                    }
                    break;
                case MotionState.Valid:
                    // 如果生效了，则判断是否回到初始位置，如果是，则识别成功
                    if (data.CompareThresholdY(JointType.HandLeft, JointType.Spine, 0) == -1)
                    {
                        _motionState = MotionState.Initial;
                        _dtEnd = DateTime.Now; // 获取截止时间
                        // 如果时间差小于一定的阈值则识别
                        if (_dtEnd.Subtract(_dtBegin).TotalMilliseconds < TimeMillisecondsThreshold)
                        {
                            return 1;
                        }
                    }
                    break;
                default:
                    // 其他状态回到了初始位置，则重置状态为初始状态
                    if (data.CompareThresholdY(JointType.HandLeft, JointType.Spine, 0) == -1)
                    {
                        _motionState = MotionState.Initial;
                    }
                    break;
            }
            if (_motionState != MotionState.Invalid)
            {
                // 如果手抬起太高，则抬起失效
                if (data.CompareThresholdY(JointType.HandLeft, JointType.Head, 5) == 1)
                {
                    _motionState = MotionState.Invalid;
                }

                // 超出身体指定的范围标记为无效，此时刚好为到Y轴的距离
                if (data.HandLeft2BodyHorizontalDistance > data.Head2HipCenterHeight)
                {
                    _motionState = MotionState.Invalid;
                }
            }
            return 0;
        }
        protected override MotionType SendCommand(int option)
        {
            return MotionType.HandLeftDown;
        }
    }
}