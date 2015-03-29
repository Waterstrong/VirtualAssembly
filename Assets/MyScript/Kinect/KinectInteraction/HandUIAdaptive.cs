using UnityEngine;
using System.Collections;

using JointType = Kinect.NuiSkeletonPositionIndex;

public class HandUIAdaptive : MonoBehaviour {

    public HandUICtrl handLeftCtrl = null; // hand ui ctrl script
    public HandUICtrl handRightCtrl = null;

    public float handMoveSmooth = 15f; // smooth arg, work with delta time

    private bool rightHandStatus = false; // hand status 
    private bool leftHandStatus = false;

    private Vector3 originR = Vector3.zero; // Right hand origin
    private Vector3 originL = Vector3.zero; // Left hand origin

    /// <summary>
    /// 通过左右手来控制适应UI操作
    /// </summary>
    /// <param name="sw"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool Adaptive(SkeletonWrapper sw, int player)
    {
        if (!gameObject.activeSelf) return false;

        if (sw.rawBonePos[player, (int)JointType.HandRight].y > sw.rawBonePos[player, (int)JointType.Spine].y)
        {
            rightHandStatus = true;
        }
        else
        {
            rightHandStatus = false;
            handRightCtrl.ResetPosition();
        }
        if (sw.rawBonePos[player, (int)JointType.HandLeft].y > sw.rawBonePos[player, (int)JointType.Spine].y)
        {
            leftHandStatus = true;
        }
        else
        {
            leftHandStatus = false;
            handLeftCtrl.ResetPosition();
        }

        if (!rightHandStatus && !leftHandStatus)
        {
            return false;
        }

        // use for the virtual hand mouse plane
        float moveOffsetThread = sw.rawBonePos[player, (int)JointType.ShoulderCenter].y - sw.rawBonePos[player, (int)JointType.Spine].y;
        float pushOffsetThread = moveOffsetThread;//sw.rawBonePos[player, (int)JointType.ShoulderCenter].y - sw.rawBonePos[player, (int)JointType.Hip].y;

        //float head2ShoulderCenterHeight = 0.5f * (sw.rawBonePos[player, (int)JointType.Head].y - sw.rawBonePos[player, (int)JointType.ShoulderCenter].y);

        // both hand y and z
        originR.y = originL.y = (sw.rawBonePos[player, (int)JointType.Head].y + sw.rawBonePos[player, (int)JointType.Spine].y) / 2;
        originR.z = originL.z = sw.rawBonePos[player, (int)JointType.Head].z;

        if (rightHandStatus && leftHandStatus)
        {
            moveOffsetThread *= 0.7f;

            originR.x = sw.rawBonePos[player, (int)JointType.Spine].x + moveOffsetThread;
            originL.x = sw.rawBonePos[player, (int)JointType.Spine].x - moveOffsetThread;
            Vector3 uiHandR = sw.rawBonePos[player, (int)JointType.HandRight] - originR;
            Vector3 uiHandL = sw.rawBonePos[player, (int)JointType.HandLeft] - originL;
            handRightCtrl.UpdateMovePush(uiHandR, moveOffsetThread, pushOffsetThread, handMoveSmooth, 1);
            handLeftCtrl.UpdateMovePush(uiHandL, moveOffsetThread, pushOffsetThread, handMoveSmooth, -1);
            //// make left and right hand not cross
            //if (handRightCtrl.transform.localPosition.x < handLeftCtrl.transform.localPosition.x)
            //{
            //    handLeftCtrl.transform.localPosition = new Vector3(handRightCtrl.transform.localPosition.x,
            //        handLeftCtrl.transform.localPosition.y, handLeftCtrl.transform.localPosition.z);
            //}
        }
        else
        {
            moveOffsetThread *= 0.5f;
            if (rightHandStatus)
            {
                originR.x = sw.rawBonePos[player, (int)JointType.Spine].x + moveOffsetThread;
                handRightCtrl.UpdateMovePush(sw.rawBonePos[player, (int)JointType.HandRight] - originR, moveOffsetThread, pushOffsetThread, handMoveSmooth);
            }
            else if (leftHandStatus)
            {
                originL.x = sw.rawBonePos[player, (int)JointType.Spine].x - moveOffsetThread;
                handLeftCtrl.UpdateMovePush(sw.rawBonePos[player, (int)JointType.HandLeft] - originL, moveOffsetThread, pushOffsetThread, handMoveSmooth);
            }
        }
        return true;
    }

    public void ShowSelf(bool isShow) {
        if ((isShow && !gameObject.activeSelf) || (!isShow && gameObject.activeSelf)) {
            gameObject.SetActive(isShow);
        }
    }
}
