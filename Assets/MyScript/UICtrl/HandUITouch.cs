using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// When ui hand collider with the other object
/// </summary>
public class HandUITouch : MonoBehaviour 
{
    private string handFreeSpriteName; // release hand sprite
    public string handPushSpriteName; // push sth sprite
    public string handGrabSpriteName; // grab sth sprite

    //private UnityEngine.Sprite handFreeSprite = null;  // release hand sprite, but private
    //public UnityEngine.Sprite handPushSprite = null; // push sth sprite
    //public UnityEngine.Sprite handGrabSprite = null; // grab sth sprite

    public bool pixelSnap = false; // if pixel snap

    private UISprite uiSprite = null; // Current using sprite
    private UI2DSprite uiSprite2D = null;

    /// <summary>
    /// Save collider which enter at the same time. but only get first one
    /// </summary>
    List<Collider> colliders = new List<Collider>(); 

    protected Collider target = null; // which object to collider
    protected NUIElement[] nuiElements = null; // attach the target script

    protected Vector3 rawPosition = Vector3.zero;

    protected bool touchTrigger = false; // identify hand is collider with other object
    public bool TouchTrigger
    {
        get { return touchTrigger; }
    }
    protected bool pushDragMove = false; // push and drag move
    public bool PushDragMove
    {
        get { return pushDragMove; }
    }
    protected bool dragMove = false;

    void Awake()
    {
        UIWidget uiWidget = gameObject.GetComponent<UIWidget>();
        uiSprite = (uiWidget as UISprite);
        uiSprite2D = (uiWidget as UI2DSprite);
        if (uiSprite != null) handFreeSpriteName = uiSprite.spriteName;
        //if (uiSprite2D != null) handFreeSprite = uiSprite2D.sprite2D;
        
    }
    void Start()
    {
        
    }


    // Use this for update
    void Update()
    {
        DragMoveTarget();
    }


    /// <summary>
    /// move the target as hand move
    /// </summary>
    public void DragMoveTarget()
    {
        if (dragMove && target != null)
        {
            // 针对当前的每个都移动
            foreach (NUIElement element in nuiElements)
            {
                if (element.PushDragMove)
                {
                    element.tweenTarget.transform.position = gameObject.transform.position;
                }
            }

            ////针对整体移动
            //target.transform.localPosition = gameObject.transform.localPosition;
        }

    }
    /// <summary>
    /// when push ui hand
    /// </summary>
    public void Pushed()
    {
        SetSprite(handPushSpriteName);
        rawPosition = target.transform.localPosition; // record the raw pos
        dragMove = pushDragMove;
        if (nuiElements != null) foreach (NUIElement element in nuiElements) element.Press(); // press element 
    }

    /// <summary>
    /// release the ui hand
    /// </summary>
    /// <param name="targetReset"></param>
    public void Release(bool targetReset = false)
    {
        SetSprite(handFreeSpriteName);
        dragMove = false;
        if (targetReset && target != null && rawPosition != Vector3.zero) // if the hand is disappeared, the reset the element position
        {
            target.transform.localPosition = rawPosition;
        }
        rawPosition = Vector3.zero;
        if (nuiElements != null) foreach (NUIElement element in nuiElements) element.Hover();

    }

    /// <summary>
    /// Collider on trigger enter
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        if (target == null )
        {
            if (!dragMove)
            {
                touchTrigger = true;
                target = other;
                //nuiElement = other.transform.GetComponent<NUIElement>();
                nuiElements = other.gameObject.GetComponents<NUIElement>();
                
                pushDragMove = false; // not drag move
                if (nuiElements != null)
                {
                    foreach (NUIElement element in nuiElements)
                    {
                        if (element.PushDragMove)
                        {
                            pushDragMove = true;
                        }
                        element.Hover();
                    }
                }

            }
        }
        else if (target != other)
        {
            colliders.Add(other);
        }

    }

    /// <summary>
    /// when not collider
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerExit(Collider other)
    {
        if (target == other) // when release hand and exit collider object
        {
            if (!dragMove)
            {
                touchTrigger = false;
                target = null;
                if (nuiElements != null)
                {
                    foreach (NUIElement element in nuiElements) element.Release(); // .Normal();Waterstrong
                    nuiElements = null;
                }

                rawPosition = Vector3.zero;

                if (colliders.Count > 0) // get the other collider 
                {
                    OnTriggerEnter(colliders[0]);
                    colliders.RemoveAt(0);
                }
            }
        }
        else
        {
            colliders.Remove(other);
        }
    }

    /// <summary>
    /// set sprite via sprite name
    /// </summary>
    /// <param name="sp"></param>
    protected void SetSprite(string sp)
    {
        if (uiSprite != null && !string.IsNullOrEmpty(sp) && uiSprite.spriteName != sp)
        {
            uiSprite.spriteName = sp;
            if (pixelSnap) uiSprite.MakePixelPerfect();
        }
    }

    /// <summary>
    /// set sprite via sprite
    /// </summary>
    /// <param name="sp"></param>
    protected void SetSprite(UnityEngine.Sprite sp)
    {
        if (sp != null && uiSprite2D != null && uiSprite2D.sprite2D != sp)
        {
            uiSprite2D.sprite2D = sp;
            if (pixelSnap) uiSprite2D.MakePixelPerfect();
        }
    }



}
