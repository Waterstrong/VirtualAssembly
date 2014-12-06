using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NUI.Motion
{
    using JointType = Kinect.NuiSkeletonPositionIndex;
    /// <summary>
    /// 双手平推
    /// </summary>
    partial class HandsMiddle : MotionSuper
    {
        protected override int Recognise(FeatureData data)
        {
            switch (_motionState)
            {
                case MotionState.Initial:
                    // 手超过了脊椎的位置
                    if (data.CompareThresholdY(JointType.HandRight, JointType.Spine, 0) == 1 &&
                        data.CompareThresholdY(JointType.HandLeft, JointType.Spine, 0) == 1)
                    {
                        // 用肩中心到脊椎的距离替代肩宽，理论上比肩宽大一些
                        // 并且两手距离近
                        if (data.Hand2HandHorizontalDistance <  data.ShoulderCenter2SpineHeight)
                        {
                            _motionState = MotionState.Valid;
                        }
                        //else
                        //{
                        //    _motionState = MotionState.Invalid; // W.S.
                        //}
                    }
                    break;
                case MotionState.Valid:
                    // 上身的高度，用于近视计算手的比例 WS (此处不知双手平推和双手展开的区别，先区别一个双手交叉吧)
                    // 当两手距离超过阈值后识别成功
                    if (data.Hand2HandHorizontalDistance > 2 * data.ShoulderCenter2HipCenterHeight )
                    {
                        //_motionState = MotionState.Invalid; // 无论是否成功识别者标记为无效状态 W.S.
                        _motionState = MotionState.Initial; // 为了多次识别
                        // 区别于双手向下的动作,只有在肩的上方
                        if (data.CompareThresholdY(JointType.HandRight, JointType.ShoulderRight, 5) >= 0 &&
                            data.CompareThresholdY(JointType.HandLeft, JointType.ShoulderLeft, 5) >= 0 )
                        {
                            return 1;
                        }
                    }
                    break;
                default:
                    break;
            }
            // 如果手抬起太高，则抬起失效
            if (_motionState != MotionState.Invalid &&
                (data.CompareThresholdY(JointType.HandRight, JointType.ShoulderCenter, 15) == 1 ||
                data.CompareThresholdY(JointType.HandLeft, JointType.ShoulderCenter, 15) == 1))
            {
                _motionState = MotionState.Invalid;
            }
            // 当手放下时恢复初始
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
            return MotionType.HandsMiddle;
        }
    }
}
