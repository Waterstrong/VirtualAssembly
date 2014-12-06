using UnityEngine;
using System.Collections;

public class HandPush  {

    public enum PushState
    {
        Moving,
        Pushing,
        Pulling,
        Pushed,
        Release
    }
    PushState pushState = PushState.Moving; // temp var to store push state

    //Vector3.zero; // Pre hand position,but remeber set z = float.maxvalue; //preUiHand.z = float.MaxValue; // don't forget
    private Vector3 preUiHand = Vector3.zero;//new Vector3(0, 0, float.MaxValue); 
    private Vector3 diffUiHand; // diff from pre hand to current hand
    private float handPushSum = 0f; // push total sum
    private readonly float HandPushThresold = 0.06f; // hand push thresold, total sum , raw 0.1f
    //private readonly float HandMoveThresold = 0.01f; // hand move thresold, but use push delta is better
    private readonly float HandPushDelta = 0.26f;// HandPushDelta*deltatime,  about 0.01f, delta push distance within every delta time, raw 0.3f

    /// <summary>
    /// used for hand pushing
    /// </summary>
    /// <param name="uiHand">real hand position</param>
    /// <param name="recTrigger">recognise trigger, if true can push else can't push</param>
    /// <returns></returns>
    public PushState Recognise2(Vector3 uiHand, bool recTrigger = false)
    {
        uiHand.z = -uiHand.z;
        if (recTrigger) // if hand if above sth handCtr.IsTrigger
        {
            if (preUiHand == Vector3.zero)
            {
                handPushSum = 0f;
                pushState = PushState.Moving;
                preUiHand = uiHand;
                return PushState.Moving;
            }
            diffUiHand = uiHand - preUiHand; // calculate the differ between last and this frame
            float pushDelta = HandPushDelta * Time.deltaTime;
            if (diffUiHand.z >= pushDelta)
            {
                preUiHand = uiHand; // get last frame position
                if (pushState != PushState.Pushed)
                {
                    handPushSum += diffUiHand.z;
                    if (handPushSum >= HandPushThresold)
                    {
                        handPushSum = 0f;
                        pushState = PushState.Pushed;
                        return PushState.Pushed;
                    }
                }
                return PushState.Pushing;
            }
            else if (diffUiHand.z <= -pushDelta) // if hand pull then push
            {
                preUiHand = uiHand; // get last frame position
                handPushSum -= diffUiHand.z ;
                if (handPushSum >= HandPushThresold)
                {
                    handPushSum = 0f;
                    pushState = PushState.Release;
                    return PushState.Release;
                }
                return PushState.Pulling;
            }
            else // make hand above apoint button
            {
                if (pushState != PushState.Moving) // if not pushed
                {
                    if (Mathf.Abs(diffUiHand.x) > pushDelta || Mathf.Abs(diffUiHand.y) > pushDelta)// if move hand on x or y, then reset prehand
                    {
                        preUiHand = uiHand; // get last frame position
                        handPushSum = 0f;
                        pushState = PushState.Moving;
                    }
                    else
                    {
                        return PushState.Pushing; // do not move avatar hand, stay on enter button
                    }
                }
                else
                {
                    preUiHand = uiHand; // get last frame position
                }
            }
        }
        else // if hand is enter nothing
        {
            handPushSum = 0f;
            pushState = PushState.Moving;
            preUiHand = uiHand; // if within delta time the press distance < deltapressthresold , then reset prehand
        }
        return PushState.Moving;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uiHand"></param>
    /// <param name="recTrigger"></param>
    /// <returns></returns>


    /// <summary>
    /// used for hand pushing
    /// </summary>
    /// <param name="uiHand">real hand position, releative to body</param>
    /// <param name="uiPushOffset">push offset refers to how far can recognise push gesture</param>
    /// <param name="recTrigger">recognise trigger, if true can push else can't push</param>
    /// <returns></returns>
    public PushState Recognise3(Vector3 uiHand, float uiPushOffset, bool recTrigger = false)
    {
        uiHand.z = -uiHand.z; // make direction reverse, origin is body
        if (recTrigger) // if hand if above sth handCtr.IsTrigger
        {
            //if (pushState == PushState.Pushed)
            //{
            //    preUiHand = uiHand; // get last frame position
            //    return PushState.Moving;
            //}
            if (preUiHand == Vector3.zero)
            {
                handPushSum = 0f;
                preUiHand = uiHand;
                pushState = PushState.Moving;
                return PushState.Moving;
            }
            diffUiHand = uiHand - preUiHand; // calculate the differ between last and this frame
            float pushDelta = HandPushDelta * Time.deltaTime;
            
            if (diffUiHand.z >= pushDelta)
            {
                preUiHand = uiHand; // get last frame position
                if (pushState != PushState.Pushed)
                {
                    handPushSum += diffUiHand.z;
                    if (handPushSum >= HandPushThresold && uiHand.z > uiPushOffset) //must greater than push offset
                    {
                        handPushSum = 0f;
                        pushState = PushState.Pushed;
                        return PushState.Pushed;
                    }
                }
                return PushState.Pushing;
            }
            else if (diffUiHand.z <= -pushDelta) // if hand pull 
            { 
                preUiHand = uiHand; // get last frame position
                handPushSum -= diffUiHand.z;// calc pull hand sum
                if (handPushSum >= HandPushThresold || uiHand.z < uiPushOffset/2) // when handZ less than push offset
                {
                    handPushSum = 0f;
                    pushState = PushState.Release;
                    return PushState.Release;
                }
                return PushState.Pulling;
            }
            else
            {
                if (pushState != PushState.Moving)
                {
                    if (Mathf.Abs(diffUiHand.x) > pushDelta || Mathf.Abs(diffUiHand.y) > pushDelta)
                    {
                        preUiHand = uiHand; // get last frame position
                        handPushSum = 0f;
                        pushState = PushState.Moving;
                    }
                    else
                    {
                        return PushState.Pushing;
                    }
                }
                else
                {
                    preUiHand = uiHand; // get last frame position
                }
            }
        }
        else
        {
            handPushSum = 0f;
            pushState = PushState.Moving;
            preUiHand = uiHand; // if within delta time the press distance < deltapressthresold , then reset prehand
        }

        return PushState.Moving;
    }


    bool isHandPushed = false;
    bool isHandMoving = true;

    /// <summary>
    /// used for hand pushing moving
    /// </summary>
    /// <param name="uiHand">real hand position, releative to body</param>
    /// <param name="uiPushOffset">push offset refers to how far can recognise push gesture</param>
    /// <param name="recTrigger">recognise trigger, if true can push else can't push</param>
    /// <param name="dragMove">if drag move , after pushed object can be dragged</param>
    /// <returns></returns>
    public PushState Recognise(Vector3 uiHand, float uiPushOffset, bool recTrigger = false, bool dragMove = false)
    {
        uiHand.z = -uiHand.z; // make direction reverse, origin is body
        if (recTrigger) // if hand if above sth handCtr.IsTrigger
        {
            //if (pushState == PushState.Pushed)
            //{
            //    preUiHand = uiHand; // get last frame position
            //    return PushState.Moving;
            //}
            if (preUiHand == Vector3.zero)
            {
                handPushSum = 0f;
                preUiHand = uiHand;
                isHandPushed = false;
                isHandMoving = true;
                return PushState.Moving;
            }
            diffUiHand = uiHand - preUiHand; // calculate the differ between last and this frame
            float pushDelta = HandPushDelta * Time.deltaTime;

            if (diffUiHand.z >= pushDelta)
            {
                preUiHand = uiHand; // get last frame position
                if (!isHandPushed)
                {
                    handPushSum += diffUiHand.z;
                    if (handPushSum >= HandPushThresold && uiHand.z > uiPushOffset) //must greater than push offset
                    {
                        handPushSum = 0f;
                        isHandPushed = true;
                        isHandMoving = false;
                        return PushState.Pushed;
                    }
                    return PushState.Pushing;
                }
                // when pushed but not moving, don't move, else moving
                return isHandMoving == false ? PushState.Pushing : PushState.Moving;
            }
            else if (diffUiHand.z <= -pushDelta) // if hand pull 
            {
                preUiHand = uiHand; // get last frame position
                if (isHandPushed) // if hand is pushed, then release
                {
                    handPushSum -= diffUiHand.z;// calc pull hand sum
                    if (handPushSum >= HandPushThresold || uiHand.z < uiPushOffset / 2) // when handZ less than push offset
                    {
                        handPushSum = 0f;
                        isHandPushed = false;
                        isHandMoving = true;
                        //preUiHand = uiHand; 
                        return PushState.Release;
                    }
                }
                else
                {
                    isHandMoving = false; // if not push, i want to pull then push, hand should not move
                }
                return PushState.Pulling;
            }
            else
            {
                if (!isHandMoving)
                {
                    if (Mathf.Abs(diffUiHand.x) > pushDelta || Mathf.Abs(diffUiHand.y) > pushDelta)
                    {
                        preUiHand = uiHand; // get last frame position
                        handPushSum = 0f;
                        isHandMoving = true;

                        if (!dragMove)// if not drag move , then release hand , not pushed
                        {
                            isHandPushed = false;
                            return PushState.Release;
                        }
                    }
                    else
                    {
                        return PushState.Pushing;
                    }
                }
                else
                {
                    preUiHand = uiHand; // get last frame position
                }
            }
        }
        else
        {
            handPushSum = 0f;
            ////isHandPushed = false; be carefull, dont set false
            isHandMoving = true;
            preUiHand = uiHand; // if within delta time the press distance < deltapressthresold , then reset prehand
        }

        return PushState.Moving;
    }


}
