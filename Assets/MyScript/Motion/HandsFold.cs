using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NUI.Motion
{
    using JointType = Kinect.NuiSkeletonPositionIndex;
    /// <summary>
    /// 双手水平收缩
    /// </summary>
    partial class HandsFold : MotionSuper
    {
        protected override int Recognise(FeatureData data)
        {
            switch (_motionState)
            {
                case MotionState.Initial:
                    // 手超过了脊椎的位置
                    if (data.CompareThresholdY(JointType.HandRight, JointType.Spine, 0) == 1 &&
                        data.CompareThresholdY(JointType.HandLeft, JointType.Spine, 0) == 1 )
                    {
                        // 并且两手距离很远(上半身的高度，用于近视计算手的比例)
                        if (data.Hand2HandHorizontalDistance > 2 * data.ShoulderCenter2HipCenterHeight)
                        {
                            _motionState = MotionState.Valid;
                        }
                        else
                        {
                            _motionState = MotionState.Invalid;
                        }
                    }
                    break;
                case MotionState.Valid:
                    // 用肩中心到脊椎的距离替代肩宽，理论上比肩宽大一些
                    if (data.Hand2HandHorizontalDistance < data.ShoulderCenter2SpineHeight)
                    {
                        _motionState = MotionState.Invalid; // 识别后标记为无效状态
                        return 1;
                    }
                    break;
                default:
                    break;
            }
            // 如果手抬起太高，则抬起失效
            if (_motionState != MotionState.Invalid &&
                (data.CompareThresholdY(JointType.HandRight, JointType.Head, 0) == 1 ||
                data.CompareThresholdY(JointType.HandLeft, JointType.Head, 0) == 1))
            {
                _motionState = MotionState.Invalid;
            }
            // 当手放下时恢复初始状态
            if (_motionState != MotionState.Initial)
            {
                if (data.CompareThresholdY(JointType.HandRight, JointType.Spine, 0) == -1 ||
                    data.CompareThresholdY(JointType.HandLeft, JointType.Spine, 0) == -1)
                {
                    _motionState = MotionState.Initial;
                }
            }
            return 0;
        }
        protected override MotionType SendCommand(int option)
        {
            return MotionType.HandsFold;
        }

    }
}
