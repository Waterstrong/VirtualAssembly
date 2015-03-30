using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NUI.Motion
{
    using JointType = Kinect.NuiSkeletonPositionIndex;
    /// <summary>
    /// 双手向下推
    /// </summary>
    partial class HandsDown : MotionSuper
    {
        private const double TimeMillisecondsThreshold = 800d; // 时间阈值，以毫秒计算
        protected override int Recognise(FeatureData data)
        {
            // 上身的高度，用于近视计算手的比例
            float maxHorizontalDistanceThreshold = 2 * data.ShoulderCenter2HipCenterHeight;

            switch (_motionState)
            {
                case MotionState.Initial:
                    // 手超过了脊椎的位置
                    if (data.CompareThresholdY(JointType.HandRight, JointType.Spine, 0) == 1 &&
                        data.CompareThresholdY(JointType.HandLeft, JointType.Spine, 0) == 1)
                    {
                        // 并且两手距离不远,用max因为向下的手势比较难识别，所以范围大
                        if (data.Hand2HandHorizontalDistance < maxHorizontalDistanceThreshold)
                        {
                            _dtBegin = DateTime.Now; // 获取开始时间
                            _motionState = MotionState.Valid;
                        }
                        else // 区别于双手收缩
                        {
                            _motionState = MotionState.Invalid;
                        }
                    }
                    break;
                case MotionState.Valid:
                    // 判断是否低于一定的位置
                    if (data.CompareThresholdY(JointType.HandRight, JointType.Spine, 0) == -1 &&
                        data.CompareThresholdY(JointType.HandLeft, JointType.Spine, 0) == -1)
                    {
                        _dtEnd = DateTime.Now;
                        // 如果时间差小于一定的阈值则识别
                        if (_dtEnd.Subtract(_dtBegin).TotalMilliseconds < TimeMillisecondsThreshold)
                        {
                            // 当两手距离超过阈值后识别成功
                            if (data.Hand2HandHorizontalDistance > maxHorizontalDistanceThreshold)
                            {
                                _motionState = MotionState.Initial; // 识别后标记为初始状态
                                return 1;
                            }
                        }
                        else
                        {
                            _motionState = MotionState.Invalid; //时间到了后无效
                        }
                    }
                    else if (data.Hand2HandHorizontalDistance > maxHorizontalDistanceThreshold &&
                        data.CompareThresholdY(JointType.HandRight, JointType.ShoulderRight, 5) >= 0 &&
                        data.CompareThresholdY(JointType.HandLeft, JointType.ShoulderLeft, 5) >= 0)
                    {
                        _motionState = MotionState.Invalid;
                    }
                    //else if (data.CompareThresholdY(JointType.HandRight, JointType.HipRight, 14) == -1 || // 加入时间戳后需要重置
                    //    data.CompareThresholdY(JointType.HandLeft, JointType.HipLeft, 14) == -1)
                    //{
                    //    _motionState = MotionState.Invalid;
                    //}
                    break;
                default:
                    // 当手放下时恢复初始
                    if (data.CompareThresholdY(JointType.HandRight, JointType.Spine, 0) == -1 ||
                        data.CompareThresholdY(JointType.HandLeft, JointType.Spine, 0) == -1)
                    {
                        _motionState = MotionState.Initial;
                    }
                    break;
            }
            // 如果手抬起太高，则抬起失效
            if (_motionState != MotionState.Invalid &&
                (data.CompareThresholdY(JointType.HandRight, JointType.Head, 0) == 1 ||
                 data.CompareThresholdY(JointType.HandLeft, JointType.Head, 0) == 1))
            {
                _motionState = MotionState.Invalid;
            }
            return 0;
        }
        protected override MotionType SendCommand(int option)
        {
            return MotionType.HandsDown;
        }
    }
}
