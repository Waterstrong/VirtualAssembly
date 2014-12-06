using UnityEngine;
using System.Collections;

namespace NUI.Motion
{
    public class HandMove
    {
        /// <summary>
        /// UI Hand tracking Control, adaptive both hands in screen
        /// </summary>
        /// <param name="uiHand">Real hand position</param>
        /// <param name="uiMoveOffset">hand move offset</param>
        /// <param name="handStatus">0 refers one hand, else both hand used and -1 refers left hand, +1 refers to right hand</param>
        /// <returns>new position in screen</returns>
        public Vector3 Tracking(Vector3 uiHand, float uiMoveOffset, int handStatus = 0, float widthBorder = 45f)
        {
            //float coeffX = Screen.width / uiWidth; // coeff on width
            //float coeffY = Screen.height / uiWidth; // coeff on height
            uiHand.x *= Screen.width / uiMoveOffset; // coeff on width
            uiHand.y *= Screen.height / uiMoveOffset; // coeff on height
            uiHand.z = 0f;

            float halfScreenWidth = Screen.width * 0.5f + 25f;// +25f;
            float halfScreenHeight = Screen.height * 0.5f + 25f;// +25f;
            float widthLeftBorder = -halfScreenWidth;
            float widthRightBorder = halfScreenWidth;

            if (handStatus != 0)
            {
                uiHand.x /= 2; // still coeff
                if (handStatus > 0)
                {
                    widthLeftBorder = widthBorder;
                    uiHand.x += Screen.width / 4; // offset the width
                }
                else
                {
                    widthRightBorder = -widthBorder;
                    uiHand.x -= Screen.width / 4;
                }
            }

            // make sure  within screen
            if (uiHand.x > widthRightBorder)
            {
                uiHand.x = widthRightBorder;
            }
            else if (uiHand.x < widthLeftBorder)
            {
                uiHand.x = widthLeftBorder;
            }
            if (uiHand.y > halfScreenHeight)
            {
                uiHand.y = halfScreenHeight;
            }
            else if (uiHand.y < -halfScreenHeight)
            {
                uiHand.y = -halfScreenHeight;
            }

            return uiHand;
        }
    }
}
