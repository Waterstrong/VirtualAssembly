using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace NUI.Motion
{
    using JointType = Kinect.NuiSkeletonPositionIndex;

    /// <summary>
    /// 双脚起跳
    /// </summary>
    partial class Jump : MotionSuper
    {
        // 假设脚踩着的面为地面，无论是在椅子上还是在跳板上或是在地上
        // 特别注意，如果在FeatureData实现的话会出现每次重新生成对象后值全部初始化，相当于没有计算
        private float _groundLevel = float.MaxValue; 
        private const float MaxHeightThreshold = 7f; // 脚最多相差的高度，一旦超过准备更新地面
        private const float MinHeightThreshold = 3f; // 最小相差角度，只有小于此范围才重置地面
        private const float JumpHeightThreshold = 5f; // 起跳的高度阈值
        private const float CounterThreshold = 120; // 用于计数判断
        private int _counter = 0;
        protected override int Recognise(FeatureData data)
        {
            // 计算两脚高度差
            float ankleDifferY = Math.Abs(data.CalculateDifferY(JointType.AnkleRight, JointType.AnkleLeft));
            // 计算两脚的绝对坐标
            float absoluteAnkleRightY = data.RelativeJoints[(int)JointType.AnkleRight].y + data.AbsoluteOrigin.y;
            float absoluteAnkleLeftY = data.RelativeJoints[(int)JointType.AnkleLeft].y + data.AbsoluteOrigin.y;
            //bool tracked = data.IsJointTracked(KinectWrapper.NuiSkeletonPositionIndex.AnkleRight); Waterstrong
            // 两脚绝对坐标Y的较小值
            float minAbsoluteAnkleY = Math.Min(absoluteAnkleLeftY, absoluteAnkleRightY);

            // 当一只脚超过另一脚时
            if (ankleDifferY > MaxHeightThreshold)
            {
                // 当想站上某个高处时即开始准备重设地面，获取较大值
                _groundLevel = Math.Max(absoluteAnkleLeftY, absoluteAnkleRightY);
            }
            switch(_motionState)
            {
                case MotionState.Initial:
                    // 当两脚高度差不超过阈值时
                    if (ankleDifferY < MinHeightThreshold)
                    {
                        // 获取左右脚以及地面中最小的值来重设地面
                        _groundLevel = Math.Min(minAbsoluteAnkleY, _groundLevel);
                        // 直接判断
                        if (minAbsoluteAnkleY - _groundLevel > JumpHeightThreshold /*&& tracked Waterstrong*/ )
                        {
                            // 重置地面，用于判断是否开始落下，否则跳太高会二次识别
                            _groundLevel = minAbsoluteAnkleY;
                            _counter = 0;
                            _motionState = MotionState.Invalid;
                            return 1;
                        }
                    }
                    break;
                default:                    
                    if ( minAbsoluteAnkleY < _groundLevel) // 当开始落下时才生效
                    {
                        _motionState = MotionState.Initial;
                    }
                    else if (++_counter >= CounterThreshold) // 可能跳到了一个较高的地方，需要一段时间来重置
                    {
                        _counter = 0;
                        _groundLevel = minAbsoluteAnkleY;
                        _motionState = MotionState.Initial;
                    }
                    break;
            }
            return 0;
        }
        protected override MotionType SendCommand(int option)
        {
            // Console.WriteLine("双脚起跳" + _recognizedID.ToString());
            
            return MotionType.Jump;
        }
    }
}
