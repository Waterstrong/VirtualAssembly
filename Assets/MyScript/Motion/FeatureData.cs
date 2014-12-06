using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NUI.Motion
{
    using JointType = Kinect.NuiSkeletonPositionIndex;
    /// <summary>
    /// 特征数据类
    /// </summary>
    public class FeatureData //: MonoBehaviour
    {
        #region 基本的属性和方法
        private const float Scale = 100f; // 转化m为cm
        private const int MaxSize = 20; // 最大关节点数目

        private Vector3[] _relativeJoints = new Vector3[MaxSize]; //存储相对关节坐标点及相关信息
        public Vector3[] RelativeJoints
        {
            get { return _relativeJoints; }
        }
        private Vector3 _absoluteOrigin; // 原点的绝对坐标
        public Vector3 AbsoluteOrigin
        {
            get { return _absoluteOrigin; }
        }

        private float _head2HipCenterHeight; // Head到HipCenter的高度
        public float Head2HipCenterHeight
        {
            get { return _head2HipCenterHeight; }
        }

        private float _head2SpineHeight; // Head到Spine的高度
        public float Head2SpineHeight
        {
            get { return _head2SpineHeight; }
        }

        private float _shoulderCenter2HipCenterHeight; // ShoulderCenter到HipCenter的高度
        public float ShoulderCenter2HipCenterHeight
        {
            get { return _shoulderCenter2HipCenterHeight; }
        }

        private float _shoulderCenter2SpineHeight; // ShoulderCenter到Spine的高度
        public float ShoulderCenter2SpineHeight
        {
            get { return _shoulderCenter2SpineHeight; }
        }

        private float _handRight2BodyHorizontalDistance; // 右手到身体的水平距离
        public float HandRight2BodyHorizontalDistance
        {
            get { return _handRight2BodyHorizontalDistance; }
        }
        private float _handLeft2BodyHorizontalDistance; // 左手到身体的水平距离
        public float HandLeft2BodyHorizontalDistance
        {
            get { return _handLeft2BodyHorizontalDistance; }
        }

        private float _hand2HandHorizontalDistance; // 两手之间的水平距离
        public float Hand2HandHorizontalDistance
        {
            get { return _hand2HandHorizontalDistance; }
            set { _hand2HandHorizontalDistance = value; }
        }
        /// <summary>
        /// 设置相对坐标[,]
        /// </summary>
        /// <param name="joints"></param>
        /// <param name="player"></param>
        public void SetRelativeJoints(Vector3[,] joints, int player)
        {
            // 设置绝对原点坐标，比例乘100，转化为CM
            _absoluteOrigin.x = (joints[player, (int)JointType.HipCenter].x +
                                joints[player, (int)JointType.Spine].x) / 2 * Scale;
            _absoluteOrigin.y = (joints[player, (int)JointType.ShoulderRight].y +
                                joints[player, (int)JointType.ShoulderLeft].y) / 2 * Scale;
            _absoluteOrigin.z = (joints[player, (int)JointType.ShoulderRight].z +
                                joints[player, (int)JointType.ShoulderLeft].z) / 2 * Scale;

            Vector3 position = new Vector3(); // struct陷阱，要单独用中间变量
            for (int i = 0; i < (int)KinectWrapper.NuiSkeletonPositionIndex.Count; ++i)
            {
                _relativeJoints[i] = joints[player, (int)i]; // 将所有信息都赋值
                // 注意struct陷阱，所以要单独用中间变量
                position.x = _relativeJoints[i].x * Scale - _absoluteOrigin.x;
                position.y = _relativeJoints[i].y * Scale - _absoluteOrigin.y;
                position.z = _absoluteOrigin.z - _relativeJoints[i].z * Scale;

                _relativeJoints[i] = position; // 只赋值相对坐标，其他信息不变
            }

            // 计算出常用的特征信息
            _head2HipCenterHeight = CalculateDifferY(JointType.Head, JointType.HipCenter);
            _head2SpineHeight = CalculateDifferY(JointType.Head, JointType.Spine);
            _shoulderCenter2HipCenterHeight = CalculateDifferY(JointType.ShoulderCenter, JointType.HipCenter);
            _shoulderCenter2SpineHeight = CalculateDifferY(JointType.ShoulderCenter, JointType.Spine);

            // 右手到身体(Y轴)的水平距离
            float differX = _relativeJoints[(int)JointType.HandRight].x;
            float differZ = _relativeJoints[(int)JointType.HandRight].z;
            _handRight2BodyHorizontalDistance = (float)Math.Sqrt(differX * differX + differZ * differZ);
            // 左手到身体(Y轴)的水平距离
            differX = _relativeJoints[(int)JointType.HandLeft].x;
            differZ = _relativeJoints[(int)JointType.HandLeft].z;
            _handLeft2BodyHorizontalDistance = (float)Math.Sqrt(differX * differX + differZ * differZ);

            // 两手之间的水平距离
            differX = CalculateDifferX(JointType.HandRight, JointType.HandLeft);
            differZ = CalculateDifferZ(JointType.HandRight, JointType.HandLeft);
            _hand2HandHorizontalDistance = (float)Math.Sqrt(differX * differX + differZ * differZ);

        }

        /// <summary>
        /// 设置相对坐标
        /// </summary>
        /// <param name="s"></param>
        public void SetRelativeJoints(Vector3[] joints)
        {
            // 设置绝对原点坐标，比例乘100，转化为CM
            _absoluteOrigin.x = (joints[(int)JointType.HipCenter].x +
                                joints[(int)JointType.Spine].x) / 2 * Scale;
            _absoluteOrigin.y = (joints[(int)JointType.ShoulderRight].y +
                                joints[(int)JointType.ShoulderLeft].y) / 2 * Scale;
            _absoluteOrigin.z = (joints[(int)JointType.ShoulderRight].z +
                                joints[(int)JointType.ShoulderLeft].z) / 2 * Scale;

            Vector3 position = new Vector3(); // struct陷阱，要单独用中间变量
            for (int i = 0; i < joints.Length; ++i )
            {
                _relativeJoints[i] = joints[(int)i]; // 将所有信息都赋值
                // 注意struct陷阱，所以要单独用中间变量
                position.x = _relativeJoints[i].x * Scale - _absoluteOrigin.x;
                position.y = _relativeJoints[i].y * Scale - _absoluteOrigin.y;
                position.z = _absoluteOrigin.z - _relativeJoints[i].z * Scale;

                _relativeJoints[i] = position; // 只赋值相对坐标，其他信息不变
            }

            // 计算出常用的特征信息
            _head2HipCenterHeight = CalculateDifferY(JointType.Head, JointType.HipCenter);
            _head2SpineHeight = CalculateDifferY(JointType.Head, JointType.Spine);
            _shoulderCenter2HipCenterHeight = CalculateDifferY(JointType.ShoulderCenter, JointType.HipCenter);
            _shoulderCenter2SpineHeight = CalculateDifferY(JointType.ShoulderCenter, JointType.Spine);

            // 右手到身体(Y轴)的水平距离
            float differX = _relativeJoints[(int)JointType.HandRight].x;
            float differZ = _relativeJoints[(int)JointType.HandRight].z;
            _handRight2BodyHorizontalDistance = (float)Math.Sqrt(differX * differX + differZ * differZ);
            // 左手到身体(Y轴)的水平距离
            differX = _relativeJoints[(int)JointType.HandLeft].x;
            differZ = _relativeJoints[(int)JointType.HandLeft].z;
            _handLeft2BodyHorizontalDistance = (float)Math.Sqrt(differX * differX + differZ * differZ);

            // 两手之间的水平距离
            differX = CalculateDifferX(JointType.HandRight, JointType.HandLeft);
            differZ = CalculateDifferZ(JointType.HandRight, JointType.HandLeft);
            _hand2HandHorizontalDistance = (float)Math.Sqrt(differX * differX + differZ * differZ);

        }
        #endregion

        //public bool IsJointTracked(JointType jt)
        //{
        //    if (_relativeJoints[(int)jt].TrackingState == JointTrackingState.Tracked)
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        #region 对空间信息进行获取或者操作
        /// <summary>
        /// 获得多个JointType对应的关节点的中心/质心
        /// </summary>
        /// <param name="jointTypes">参数，可以指定多个</param>
        /// <returns>返回多个关节点的中心/质心</returns>
        public Vector3 CalculateCenterPoint(params JointType[] jointTypes)
        {
            Vector3 centerPoint = new Vector3();
            if (jointTypes.Length < 1)
            {
                centerPoint.x = centerPoint.y = centerPoint.z = 0;
                return centerPoint;
            }
            float sumX = 0f;
            float sumY = 0f;
            float sumZ = 0f;
            foreach (JointType jointType in jointTypes)
            {
                sumX += _relativeJoints[(int)jointType].x;
                sumY += _relativeJoints[(int)jointType].y;
                sumZ += _relativeJoints[(int)jointType].z;
            }
            centerPoint.x = sumX / jointTypes.Length;
            centerPoint.y = sumY / jointTypes.Length;
            centerPoint.z = sumZ / jointTypes.Length;
            return centerPoint;
        }
        /// <summary>
        /// 计算多个点的空间长度距离，若有超过两个关节点，则分别计算两两距离再相加
        /// </summary>
        /// <param name="jointTypes">传入关节点类型集合</param>
        /// <returns>返回空间叠加长度</returns>
        public float CalculateSpaceDistance(params JointType[] jointTypes)
        {
            float distance = 0f;
            if (jointTypes.Length < 2)
            {
                return distance;
            }
            Vector3 prePos = _relativeJoints[(int)jointTypes[0]];
            Vector3 curPos;
            float differX, differY, differZ;
            for (int i = 1; i < jointTypes.Length; ++i)
            {
                curPos = _relativeJoints[(int)jointTypes[i]];
                differX = prePos.z - curPos.x;
                differY = prePos.y - curPos.y;
                differZ = prePos.z - curPos.z;
                distance += (float)Math.Sqrt(differX * differX + differY * differY + differZ * differZ);
                prePos = curPos;
            }
            return distance;
        }
        #endregion

        #region 对一维方向上的信息进行操作
        /// <summary>
        /// 计算两关节点在X方向上的差值
        /// </summary>
        /// <param name="jointType1">起始关节点</param>
        /// <param name="jointType2">结束关节点</param>
        /// <returns>返回在X方向上关节点1减去关节点2的值</returns>
        public float CalculateDifferX(JointType jointType1, JointType jointType2)
        {
            return (_relativeJoints[(int)jointType1].x - _relativeJoints[(int)jointType2].x);
        }
        /// <summary>
        /// 计算两关节点在Y方向上的差值
        /// </summary>
        /// <param name="jointType1">起始关节点</param>
        /// <param name="jointType2">结束关节点</param>
        /// <returns>返回在Y方向上关节点1减去关节点2的值</returns>
        public float CalculateDifferY(JointType jointType1, JointType jointType2)
        {
            return (_relativeJoints[(int)jointType1].y - _relativeJoints[(int)jointType2].y);
        }
        /// <summary>
        /// 计算两关节点在Z方向上的差值
        /// </summary>
        /// <param name="jointType1">起始关节点</param>
        /// <param name="jointType2">结束关节点</param>
        /// <returns>返回在Z方向上关节点1减去关节点2的值</returns>
        public float CalculateDifferZ(JointType jointType1,JointType jointType2)
        {
            return (_relativeJoints[(int)jointType1].z - _relativeJoints[(int)jointType2].z);
        }
        #endregion

        #region 判断关节点在某个方向上一定程序的大小关系
        /// <summary>
        /// 比较关节点在X方向上一定程序的大小
        /// </summary>
        /// <param name="jointType1">关节点1</param>
        /// <param name="jointType2">关节点2</param>
        /// <param name="threshold">判断相等条件的阈值,默认为10cm，在[-threshold,threshold]内都为相等，即自由量为2*threshold</param>
        /// <returns>返回判断结果，1表示前者明显大于后者，-1表示小于，0表示在阈值范围内</returns>
        public int CompareThresholdX(JointType jointType1, JointType jointType2, float threshold = 10f)
        {
            float differ = _relativeJoints[(int)jointType1].x - _relativeJoints[(int)jointType2].x;
            return CompareThreshold(differ, threshold);
        }
        /// <summary>
        /// 比较关节点在Y方向上一定程序的大小
        /// </summary>
        /// <param name="jointType1">关节点1</param>
        /// <param name="jointType2">关节点2</param>
        /// <param name="threshold">判断相等条件的阈值,默认为10cm，在[-threshold,threshold]内都为相等，即自由量为2*threshold</param>
        /// <returns>返回判断结果，1表示前者明显大于后者，-1表示小于，0表示在阈值范围内</returns>
        public int CompareThresholdY(JointType jointType1, JointType jointType2, float threshold = 10f)
        {
            float differ = _relativeJoints[(int)jointType1].y - _relativeJoints[(int)jointType2].y;
            return CompareThreshold(differ, threshold);
        }
        /// <summary>
        /// 比较关节点在Z方向上一定程序的大小
        /// </summary>
        /// <param name="jointType1">关节点1</param>
        /// <param name="jointType2">关节点2</param>
        /// <param name="threshold">判断相等条件的阈值,默认为10cm，在[-threshold,threshold]内都为相等，即自由量为2*threshold</param>
        /// <returns>返回判断结果，1表示前者明显大于后者，-1表示小于，0表示在阈值范围内</returns>
        public int CompareThresholdZ(JointType jointType1, JointType jointType2, float threshold = 10f)
        {
            float differ = _relativeJoints[(int)jointType1].z - _relativeJoints[(int)jointType2].z;
            return CompareThreshold(differ, threshold);
        }
        /// <summary>
        /// 比较两数差在一定程度上的是否超出某个范围
        /// </summary>
        /// <param name="differ">前者减后者的差值</param>
        /// <param name="threshold">判断的阈值，在[-threshold,threshold]内都为相等，即自由量为2*threshold</param>
        /// <returns>返回判断结果，1表示前者明显大于后者，-1表示小于，0表示在阈值范围内</returns>
        private int CompareThreshold(float differ, float threshold)
        {
            if (differ > threshold) // num1 > num2
            {
                return 1;
            }
            else if (differ < -threshold) // num1 < num2
            {
                return -1;
            }
            else // num1 与 num2 相差范围在 [-threshlod, threshlod]视为相等
            {
                return 0;
            }
        }
        #endregion
        
    }
}
