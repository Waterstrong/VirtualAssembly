using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NUI.Motion
{

    using JointType = Kinect.NuiSkeletonPositionIndex;
    /// <summary>
    /// 双手向上推
    /// </summary>
    partial class HandsUp : MotionSuper
    {
        protected override int Recognise(FeatureData data)
        {
            // 用肩中心到脊椎的距离替代肩宽，理论上比肩宽大一些
            float minHorizontalDistanceThreshold = data.ShoulderCenter2SpineHeight;
            
            switch (_motionState)
            {
                case MotionState.Initial:
                    // 手是否在经过肩附近
                    if (data.CompareThresholdY(JointType.HandRight, JointType.ShoulderRight, 15) == 0 &&
                        data.CompareThresholdY(JointType.HandLeft, JointType.ShoulderRight, 15) == 0 &&
                        data.Hand2HandHorizontalDistance < minHorizontalDistanceThreshold) // 并且两手距离很近
                    {
                       _motionState = MotionState.Valid;
                    }
                    break;
                case MotionState.Valid:
                    // 上身的高度，用于近视计算手的比例
                    // 当两手距离超过阈值后识别成功
                    if (data.Hand2HandHorizontalDistance > /*2 * */data.ShoulderCenter2HipCenterHeight)
                    {
                        // 判断是否高于头部
                        if (data.CompareThresholdY(JointType.HandRight, JointType.Head, 0) == 1 &&
                            data.CompareThresholdY(JointType.HandLeft, JointType.Head, 0) == 1)
                        {
                            _motionState = MotionState.Initial; // 识别后标记为初始状态
                            return 1;
                        }
                        else if (data.Hand2HandHorizontalDistance > 2 * data.ShoulderCenter2HipCenterHeight)// &&
                            //data.CompareThresholdY(JointType.HandRight, JointType.ShoulderRight, 15) == 0 &&
                            //data.CompareThresholdY(JointType.HandLeft, JointType.ShoulderRight, 15) == 0)
                        {
                            _motionState = MotionState.Initial; // 排除从两边抬起手臂，还可排除手低的情况
                        }
                    }
                    break;
                default:
                    break;
            }
            // 当手放下时恢复初始状态
            if (_motionState != MotionState.Initial &&
                (data.CompareThresholdY(JointType.HandRight, JointType.Spine, 0) == -1 ||
                data.CompareThresholdY(JointType.HandLeft, JointType.Spine, 0) == -1))
            {
                _motionState = MotionState.Initial;
            }
            // 当两手合在一起后超过头设置
            //if (_motionState != MotionState.Initial &&
            //    data.Hand2HandHorizontalDistance < minHorizontalDistanceThreshold &&
            //    (data.CompareThresholdY(JointType.HandRight, JointType.Head, 0) == 1 ||
            //    data.CompareThresholdY(JointType.HandLeft, JointType.Head, 0) == 1))
            //{
            //    _motionState = MotionState.Initial;
            //}
            return 0;
        }
        protected override MotionType SendCommand(int option)
        {
            return MotionType.HandsUp;
        }
    }
}
