using UnityEngine;
using System.Collections;

public class NUIElementScale : MonoBehaviour {

    public Transform tweenTarget;
    public Vector3 hover = new Vector3(1.1f, 1.1f, 1.1f);
    public Vector3 pressed = new Vector3(1.05f, 1.05f, 1.05f);
    public float duration = 0.2f;

    Vector3 mScale;
    bool mStarted = false;

    void Start()
    {
        if (!mStarted)
        {
            mStarted = true;
            if (tweenTarget == null) tweenTarget = transform;
            mScale = tweenTarget.localScale;
        }
    }

    void OnEnable() { if (mStarted) OnHover(UICamera.IsHighlighted(gameObject)); }

    void OnDisable()
    {
        if (mStarted && tweenTarget != null)
        {
            TweenScale tc = tweenTarget.GetComponent<TweenScale>();

            if (tc != null)
            {
                tc.value = mScale;
                tc.enabled = false;
            }
        }
    }

    void OnPress(bool isPressed)
    {
        if (enabled)
        {
            if (!mStarted) Start();
            TweenScale.Begin(tweenTarget.gameObject, duration, isPressed ? Vector3.Scale(mScale, pressed) :
                (UICamera.IsHighlighted(gameObject) ? Vector3.Scale(mScale, hover) : mScale)).method = UITweener.Method.EaseInOut;
        }
    }

    void OnHover(bool isOver)
    {
        if (enabled)
        {
            if (!mStarted) Start();
            TweenScale.Begin(tweenTarget.gameObject, duration, isOver ? Vector3.Scale(mScale, hover) : mScale).method = UITweener.Method.EaseInOut;
        }
    }

    void OnSelect(bool isSelected)
    {
        if (enabled && (!isSelected || UICamera.currentScheme == UICamera.ControlScheme.Controller))
            OnHover(isSelected);
    }

    public void Hover()
    {
        OnHover(true);
    }
    public void Normal()
    {
        OnDisable();
    }
    public void Press()
    {
        OnPress(true);
    }

}
