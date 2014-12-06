using UnityEngine;
using System.Collections;

public class PromptUICtrl : MonoBehaviour {

    public UILabel words = null;
    public GameObject panel = null;

    public void Show(string msg, bool showPanel = true)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        if (panel.activeSelf != showPanel)
        {
            panel.SetActive(showPanel);
        }
        words.text = msg;
    }

    public void Hide()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
        
    }
}
