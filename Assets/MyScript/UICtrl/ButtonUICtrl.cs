using UnityEngine;
using System.Collections;

public class ButtonUICtrl : MonoBehaviour {

    public UILabel caption = null;

    public bool ChangeCaption(string newCaption) {
        if(caption) {
            caption.text = newCaption;
            return true;
        }
        return false;
    }

    public void ShowSelf(bool isShow){
        if ((isShow && !gameObject.activeSelf) || (!isShow && gameObject.activeSelf)) {
            gameObject.SetActive(isShow);
        }
    }

}
