using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using System.Reflection;

namespace NUI.Motion
{
    /// <summary>
    /// 动作处理链生成工厂
    /// </summary>
    public static class MotionFactory
    {
        /// <summary>
        /// 生成职责处理链
        /// </summary>
        /// <returns>返回处理链的头部</returns>
        public static MotionSuper CreateHandleChain()
        {
            MotionSuper headMotion = null;
            MotionSuper preMotion = null;
            MotionSuper nextMotion = null;
            try
            {
                //string[] motionNames = GetMotionNames("config\\motion.ini"); // 获取配置文件中类名集合 Waterstrong
                //string[] motionNames = { "Jump", "HandLeftSlide"};
                MotionSuper[] motionSupers = { new Jump(), new HandsUp(),
                                                 new HandsFold(), new HandsMiddle(),
                                                  new HandLeftSlide(), new HandRightSlide(),
                                                   new HandLeftUp(), new HandRightUp(),
                                                   new HandLeftCircle(),  new HandRightCircle(),
                                                   new HandLeftDown(), new HandRightDown() }; // Waterstrong Alter
                int count = 0;
                //foreach (string name in motionNames) // 设置职责链
                foreach (MotionSuper motion in motionSupers) // 设置职责链 // Waterstrong Alter
                {
                    if (count++ == 0)
                    {
                        //headMotion = GetInstance(name);
                        headMotion = motion;// Waterstrong Alter
                        preMotion = headMotion;
                    }
                    else
                    {
                        //nextMotion = GetInstance(name);
                        nextMotion = motion; // Waterstrong Alter
                        preMotion.SetSuccessor(nextMotion);
                        preMotion = nextMotion;
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return headMotion;
        }
        /// <summary>
        /// 反射获取类实例
        /// </summary>
        /// <param name="motionName">传入类名字符串</param>
        /// <returns>返回实例</returns>
        private static MotionSuper GetInstance(string motionName)
        {
            if (motionName == "Jump")
            {
                return new Jump();
            }
            else if (motionName == "HandRightSlide")
            {
                return new HandRightSlide();
            }
            else if (motionName == "HandLeftSlide")
            {
                return new HandLeftSlide();
            }
            //else if (motionName == "LRIncline")
            //{
            //    return new LRIncline();
            //}
            return new Jump();// (MotionSuper)Assembly.Load("NUI.Motion").CreateInstance("NUI.Motion." + motionName);
        }
        /// <summary>
        /// 获取所有的类名
        /// </summary>
        /// <returns></returns>
        private static string[] GetMotionNames(string fileName)
        {
            string strRead;
            string[] motionNames = new string[0];
            char[] seperator = { ' ', '\n', '\r', ','}; // 换行和空格都可以分隔
            try
            {
                strRead = File.ReadAllText(fileName); // 读取配置文件
                motionNames = strRead.Split(seperator, StringSplitOptions.RemoveEmptyEntries); // 分离出类名
            }
            catch (System.Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return motionNames;
        }
    }
}
