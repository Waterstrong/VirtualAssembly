using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NUI.Motion
{
    using JointType = Kinect.NuiSkeletonPositionIndex;
    /// <summary>
    /// 手右滑动
    /// </summary>
    partial class HandRightSlide : MotionSuper
    {
        protected override int Recognise(FeatureData data)
        {
            // 如果另一只手不在初始位置，则不识别
            if (data.CompareThresholdY(JointType.HandLeft, JointType.Spine, 0) == 1)
            {
                _motionState = MotionState.Invalid;
                return 0;
            }

            switch(_motionState)
            {
                case MotionState.Initial:
                    // 用肩中心到脊椎的距离大致估计手的比例，确保身高不影响,超出身体指定的范围标记为无效，此时刚好为到Y轴的距离
                    // 手超过了脊椎的位置并且手在身体附近
                    if (data.CompareThresholdY(JointType.HandRight, JointType.Spine, 0) == 1 &&
                        data.HandRight2BodyHorizontalDistance < data.ShoulderCenter2SpineHeight )
                    {
                        _motionState = MotionState.Valid;
                    }
                    break;
                case MotionState.Valid:
                    // 上身的高度，用于近视计算手的比例
                    if (data.HandRight2BodyHorizontalDistance > data.Head2HipCenterHeight)
                    {
                        _motionState = MotionState.Initial;
                        return 1;
                    }
                    break;
                default:
                    break;
            }
            // 如果手抬起太高，则抬起失效
            if (_motionState != MotionState.Invalid &&
                data.CompareThresholdY(JointType.HandRight, JointType.Head, 0) == 1)
            {
                _motionState = MotionState.Invalid;
            }
            // 当手放下时
            if (_motionState != MotionState.Initial)
            {
                if (data.CompareThresholdY(JointType.HandRight, JointType.Spine, 0) == -1)
                {
                    _motionState = MotionState.Initial;
                }
                else if (_motionState == MotionState.Invalid // 当因为另一只手抬起导致状态无效时
                    && data.CompareThresholdY(JointType.HandLeft, JointType.Spine, 0) == -1)
                {
                    _motionState = MotionState.Initial;
                }
            }
            return 0;
        }
        protected override MotionType SendCommand(int option)
        {
            return MotionType.HandRightSlide;
        }
    }
}
