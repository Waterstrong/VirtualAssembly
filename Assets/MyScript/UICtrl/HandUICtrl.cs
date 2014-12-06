using UnityEngine;
using System.Collections;

using NUI.Motion;

public class HandUICtrl : MonoBehaviour {
    
    /// <summary>
    /// hand gesture recognition
    /// </summary>
    private HandMove handMove = new HandMove();
    private HandPush handPush = new HandPush();

    /// <summary>
    /// hand remote touch 
    /// </summary>
    private HandUITouch handTouch = null;


	// Use this for initialization
	void Start () {
        handTouch = gameObject.GetComponent<HandUITouch>();
        ResetPosition(); // reset the hand position
        
	}
    /// <summary>
    /// Reset the position
    /// </summary>
    public void ResetPosition()
    {
        if (handTouch != null) handTouch.Release(true); // Release hand and reset position(true)
        transform.localPosition = new Vector3(transform.localPosition.x, -Screen.height, transform.localPosition.z);
    }


    /// <summary>
    /// UI Hand Position Update Control
    /// </summary>
    /// <param name="uiHand">real hand position, relative to body</param>
    /// <param name="uiMoveOffset">hand move offset</param>
    /// <param name="handMoveSmooth">hand smooth for move</param>
    /// <param name="handStatus">0 refers one hand, else both hand used and -1 refers left hand, +1 refers to right hand</param>
    public void UpdateMovePush(Vector3 uiHand, float uiMoveOffset, float uiPushOffset, float handMoveSmooth = 15f, int handStatus = 0)
    {
        HandPush.PushState pushState = HandPush.PushState.Moving; // if hand touch is null then return        
        if (handTouch != null)
        {
           pushState = handPush.Recognise(uiHand, uiPushOffset, handTouch.TouchTrigger, handTouch.PushDragMove); //handTouch.TouchTrigger
        }
        switch(pushState)
        {
            case HandPush.PushState.Pushed:
                if (handTouch != null) handTouch.Pushed(); // hand pushed
                break;
            case HandPush.PushState.Release:
                if (handTouch != null) handTouch.Release(); // Hand Release
                break;
            case HandPush.PushState.Moving:
                // old pos to new pos with smooth arg
                transform.localPosition = Vector3.Lerp(this.transform.localPosition,
                    handMove.Tracking(uiHand, uiMoveOffset, handStatus), Time.deltaTime * handMoveSmooth); // Time.deltaTime * handMoveSmooth
                break;
            default: break;

        }
        
    }

}
