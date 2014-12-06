using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NUI.Motion
{
    using JointType = Kinect.NuiSkeletonPositionIndex;
    /// <summary>
    /// 手左向上
    /// </summary>
    partial class HandLeftUp : MotionSuper
    {
        protected override int Recognise(FeatureData data)
        {
            // 当另一只手也举起则无效
            if (data.CompareThresholdY(JointType.HandRight, JointType.ShoulderRight, 0) == 1)
            {
                _motionState = MotionState.Invalid;
                return 0;
            }

            switch (_motionState)
            {
                case MotionState.Initial:
                    // 当超过肩开始生效
                    if (data.CompareThresholdY(JointType.HandLeft, JointType.ShoulderRight, 0) == 1)
                    {
                        // 太近了则无效，区别于划圆
                        if ( data.CalculateDifferX(JointType.ShoulderCenter, JointType.HandLeft) < 5 ||
                            (data.HandLeft2BodyHorizontalDistance < data.ShoulderCenter2SpineHeight &&
                            data.CalculateSpaceDistance(JointType.HandLeft, JointType.ShoulderRight) < data.ShoulderCenter2SpineHeight))
                        {
                            _motionState = MotionState.Invalid;
                        }
                        else
                        {
                            _motionState = MotionState.Valid;
                        }
                    }
                    break;
                case MotionState.Valid:
                    // 手举起后肘关节与头的相对位置
//                     if (data.CompareThresholdY(JointType.ElbowLeft, JointType.Head, 13) == 0 && 
//                         data.CompareThresholdX(JointType.ElbowLeft, JointType.Head, 33) == 0)
                    if (data.CompareThresholdY(JointType.HandLeft, JointType.Head, 5) == 1)
                    {
                        _motionState = MotionState.Invalid;
                        return 1;
                    }
                    //else if (data.HandLeft2BodyHorizontalDistance > 1.1 * data.Head2HipCenterHeight) // 在有效状态下，超出身体指定的范围标记为无效，可能是从侧面举起手的
                    //{
                    //    // 水平距离的阈值,大致估计手的比例，确保身高不影响
                    //    _motionState = MotionState.Invalid;
                    //}
                    break;
                default:
                    break;
            }

            // 手放下来后恢复初始状态
            if (_motionState != MotionState.Initial &&
                data.CompareThresholdY(JointType.HandLeft, JointType.Spine, 0) == -1)
            {
                _motionState = MotionState.Initial;
            }
            return 0;
        }
        protected override MotionType SendCommand(int option)
        {
            return MotionType.HandLeftUp;
        }
    }
}
