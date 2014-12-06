
using UnityEngine;
using System.Collections.Generic;


public class NUIElement : MonoBehaviour
{

    static public NUIElement current = null;

    public GameObject tweenTarget = null;

    /// <summary>
    /// pushed, then if can drag move
    /// </summary>
    public bool PushDragMove = false;
    /// <summary>
    /// grabbed, then if can drag move
    /// </summary>
    public bool GrabDragMove = false;


    public bool enableColor = true;
    /// <summary>
    /// Color to apply on hover event (mouse only).
    /// </summary>
    public Color hoverColor = new Color(225f / 255f, 200f / 255f, 150f / 255f, 1f);

    /// <summary>
    /// Color to apply on the pressed event.
    /// </summary>
    public Color pressedColor = new Color(183f / 255f, 163f / 255f, 123f / 255f, 1f);

    /// <summary>
    /// Color that will be applied when the button is disabled.
    /// </summary>
    public Color disabledColor = Color.grey;

    private Color normalColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, 1f);
    
    /// <summary>
    /// Duration of the tween process.
    /// </summary>


    public bool enableScale = false;
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    public Vector3 pressedScale = new Vector3(1.05f, 1.05f, 1.05f);
    Vector3 mScale;

    public float duration = 0.2f;

    //[System.NonSerialized]
    //protected State mState = State.Normal;

    public List<EventDelegate> onElementPress = new List<EventDelegate>();

    public List<EventDelegate> onElementRelease = new List<EventDelegate>();

    //int hoverCnt = 0;
    //bool pressing = false;

    void Awake()
    {
        if (tweenTarget == null) tweenTarget = gameObject;
        UISprite uiSprite = tweenTarget.GetComponent<UISprite>();
        if (uiSprite != null) normalColor = uiSprite.color;
        mScale = tweenTarget.transform.localScale; // store scale
    }

    bool pressed = false;

    /// <summary>
    /// on nui hand press
    /// </summary>
    public void Press()
    {
        if (enableColor)
        {
            //change to press color
            UpdateColor(pressedColor);
        }
        if (enableScale)
        {
            OnPressScale(true);
        }

        // send click message
        //EventDelegate.Parameter paras = new EventDelegate.Parameter(this, "hell0");
        if (current == null)
        {
            current = this;
            EventDelegate.Execute(onElementPress);
            current = null;
        }

        pressed = true;
    }

    /// <summary>
    /// release  // Waterstrong 
    /// </summary>
    public void Release()
    {
        Normal();
        if (PushDragMove && pressed)
        { // Waterstrong alter
            if (current == null)
            {
                current = this;
                EventDelegate.Execute(onElementRelease);

                tweenTarget.SetActive(false);
                current = null;
            }
        }
        
    }

    /// <summary>
    /// when exit the element
    /// </summary>
    public void Normal()
    {
        if (enableColor)
        {
            UpdateColor(normalColor);
        }
        if (enableScale)
        {
            OnHoverScale(false);
        }
    }

    /// <summary>
    /// when enter the element
    /// </summary>
    public void Hover()
    {
        if (enableColor)
        {
            UpdateColor(hoverColor);
        }
        if (enableScale)
        {
            OnHoverScale(true);
        }
    }

    /// <summary>
    /// when disable the element
    /// </summary>
    public void Disable()
    {
        if (enableColor)
        {
            UpdateColor(disabledColor);
        }
        if (enableScale)
        {
            OnDisableScale();
        }
    }

    /// <summary>
    /// Update UI element color
    /// </summary>
    /// <param name="toColor"></param>
    void UpdateColor(Color toColor)
    {
        if (tweenTarget != null) TweenColor.Begin(tweenTarget, duration, toColor);
    }


    ////////////////////////////////////
 
    void OnEnableScale() {  OnHoverScale(UICamera.IsHighlighted(gameObject)); }

    void OnDisableScale()
    {
        if (tweenTarget != null)
        {
            TweenScale tc = tweenTarget.GetComponent<TweenScale>();

            if (tc != null)
            {
                tc.value = mScale;
                tc.enabled = false;
            }
        }
    }

    void OnPressScale(bool isPressed)
    {
        TweenScale.Begin(tweenTarget.gameObject, duration, isPressed ? Vector3.Scale(mScale, pressedScale) :
                (UICamera.IsHighlighted(gameObject) ? Vector3.Scale(mScale, hoverScale) : mScale)).method = UITweener.Method.EaseInOut;
    }

    void OnHoverScale(bool isOver)
    {
        if (enabled)
        {
            TweenScale.Begin(tweenTarget.gameObject, duration, isOver ? Vector3.Scale(mScale, hoverScale) : mScale).method = UITweener.Method.EaseInOut;
        }
    }

    void OnSelectScale(bool isSelected)
    {
        if (enabled && (!isSelected || UICamera.currentScheme == UICamera.ControlScheme.Controller))
            OnHoverScale(isSelected);
    }

}
