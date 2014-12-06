using UnityEngine;
using System.Collections;

namespace NUI.Motion
{
    using JointType = Kinect.NuiSkeletonPositionIndex;
    /// <summary>
    /// recongise hand wave
    /// </summary>
    public class HandWave
    {
        /////////////////////////////////////////////
        private Vector3 preWaveHand = Vector3.zero; // Pre hand position,but remeber set z = float.maxvalue;
        private Vector3 diffWaveHand; // diff from pre hand to current hand
        private Vector2 handWaveSum = Vector2.zero; // hand wave left or right sum
        private int waveRightCnt = 0; // one hand wave to  left or right times
        private int waveLeftCnt = 0;
        private int timerCnt = 0; // timer counter
        private int waveDirection = 0; // which direction to wave, if right wave success, set 1, left wave success set -1
        private readonly float HandWaveThresold = 0.15f; // hand wave thresold, total sum
        private readonly float HandWaveDelta = 0.25f;// HandWaveDelta*deltatime

        /// <summary>
        /// set the data as init
        /// </summary>
        protected void ResetData()
        {
            preWaveHand = Vector3.zero;
            handWaveSum = Vector2.zero;
            waveRightCnt = 0;
            waveLeftCnt = 0;
            timerCnt = 0;
            waveDirection = 0;
        }
        /// <summary>
        /// wave hand action recognise
        /// </summary>
        /// <param name="waveHand"></param>
        /// <returns></returns>
        protected bool Recognise(Vector3 waveHand)
        {
            if (preWaveHand == Vector3.zero)
            {
                preWaveHand = waveHand;
                return false;
            }
            diffWaveHand = waveHand - preWaveHand;
            preWaveHand = waveHand;
            float waveDelta = HandWaveDelta * Time.deltaTime;
            if (Mathf.Abs(diffWaveHand.z) > HandWaveThresold) // reset wave info when move in z greater than wave thresold
            {
                ResetData();
                return false;
            }
            if (diffWaveHand.x > waveDelta) // when move speed greater than waveDelta on x-axis
            {
                timerCnt = 0;
                if (waveDirection != 1) // identify that this direction has waved
                {
                    handWaveSum.x += diffWaveHand.x;
                    if (handWaveSum.x > HandWaveThresold) // which direction and how far hand move 
                    {
                        ++waveRightCnt;
                        handWaveSum.x = 0;
                        waveDirection = 1;
                    }
                }
            }
            else if (diffWaveHand.x < -waveDelta)
            {
                timerCnt = 0;
                if (waveDirection != -1)
                {
                    handWaveSum.y -= diffWaveHand.x;
                    if (handWaveSum.y > HandWaveThresold)
                    {
                        ++waveLeftCnt;
                        handWaveSum.y = 0;
                        waveDirection = -1;
                    }
                }
            }
            else if (++timerCnt > 0.35 / Time.deltaTime) // some seconds there is no move
            {
                ResetData();
            }
            if (waveRightCnt >= 1 && waveLeftCnt >= 1 && (waveLeftCnt + waveRightCnt) >= 3) // after hand move left and right
            {
                ResetData();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Wave to swape player , wave hand action recognise
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public  bool Recognise(SkeletonWrapper sw, int player)
        {
            if (sw.trackedPlayers[player] != -1)
            {
                if (sw.rawBonePos[player, (int)JointType.HandRight].y > sw.rawBonePos[player, (int)JointType.Spine].y)
                {
                    return Recognise(sw.rawBonePos[player, (int)JointType.HandRight]);
                }
                if (sw.rawBonePos[player, (int)JointType.HandLeft].y > sw.rawBonePos[player, (int)JointType.Spine].y)
                {
                    return Recognise(sw.rawBonePos[player, (int)JointType.HandLeft]);
                }
                ResetData();
            }
            else
            {
                ResetData();
            }
            return false;
        }
    }

}
