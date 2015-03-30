using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NUI.Motion
{
    /// <summary>
    /// 动作枚举类型
    /// </summary>
    public enum MotionType
    {
        None = 0,
        Jump,
        HandLeftSlide,
        HandRightSlide,
        HandsUp,
        HandsMiddle,
        HandsFold,
        HandLeftUp,
        HandRightUp,
        HandLeftCircle,
        HandRightCircle,
        FootLeftUp,
        FootRightUp,
        HandLeftDown,
        HandRightDown,
        HandsDown,
        LegLeftUp,
        LegRightUp
    }

    /// <summary>
    /// 动作定义的超类，职责链模式
    /// </summary>
    public abstract class MotionSuper
    {
        protected Int64 _recognizedID = 0; // 控制识别编号

        protected DateTime _dtBegin; // 时间戳开始
        protected DateTime _dtEnd; // 时间戳结束

        // protected int _recognizedCount = 0; // 控制识别一定数目结束后才发送

        // protected const int RecognizedCountThreshold = 3; // 连续识别成功阈值

        protected MotionState _motionState = MotionState.Initial;

        protected MotionSuper _successor = null; // 下一个处理者

        /// <summary>
        /// 设置下一个处理者
        /// </summary>
        /// <param name="successor">后继处理者</param>
        public void SetSuccessor(MotionSuper successor)
        {
            _successor = successor;
        }


        /// <summary>
        /// 处理数据请求
        /// </summary>
        /// <param name="data">传入特征数据</param>
        /// <returns>返回识别代码，未识别返回-1</returns>
        public MotionType HandleDataEx(FeatureData data)
        {
            int res = Recognise(data);
            if (res != 0)
            {
                ++_recognizedID;
                return SendCommand(res);
            }
            else
            {
                if (_successor != null) // 如果存在后继者则处理
                {
                    return _successor.HandleDataEx(data);
                }
            }
            return MotionType.None;
        }
        /// <summary>
        /// 处理请求数据
        /// </summary>
        /// <param name="data">传入的特征数据</param>
        /// <returns>处理成功返回true，否则返回false</returns>
        public bool HandleData(FeatureData data)
        {
            if (Recognise(data) != 0)
            {
                //// 识别到连续RecognizedCountThreshold个成功后发送命令，而且只发送一次
                //if (++_recognizedCount == RecognizedCountThreshold) // 特别注意是先加
                //{
                //    // convert and send 
                //    SendCommand();
                //    //Thread.Sleep(1000);
                //}

                ++_recognizedID;
                // convert and send
                try
                {
                    SendCommand();
                }
                catch (System.Exception ex)
                {
                    throw new Exception(ex.Message);
                }
				return true;
            }
            else
            {
                // _recognizedCount = 0; // 恢复识别计数器
                if (_successor != null) // 如果存在后继者则处理
                {
                    return _successor.HandleData(data);
                }
            }
            return false;
        }
        /// <summary>
        /// 识别是否为当前的定义的动作
        /// </summary>
        /// <param name="data">特征数据</param>
        /// <returns>识别是否成功,+-1表示正反两种状态，0表示未识别</returns>
        protected abstract int Recognise(FeatureData data);
        /// <summary>
        /// 识别成功后发送相应命令
        /// </summary>
        protected abstract MotionType SendCommand(int option = 0);

    }
}
