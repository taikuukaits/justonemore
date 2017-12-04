using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour {

    public Text text;


    public void ShowText(string tooltip)
    {
        text.text = tooltip;
        Color c = text.color;
        c.a = 0f;
        text.color = c;

        DOTween.Sequence().Append(text.DOFade(1f, 1f)).AppendInterval(5f).Append(text.DOFade(0f, 1f));
        
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
