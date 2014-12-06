using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NUI.Motion
{
    /// <summary>
    /// 动作状态
    /// </summary>
    public enum MotionState
    {
        Initial = 0, // 初始状态
        Valid = 1, // 有效状态
        Invalid = 2, // 无效状态
        Unknown = 3, // 未知状态
        Valid2 = 4 // 第二个有效状态
    };
}
