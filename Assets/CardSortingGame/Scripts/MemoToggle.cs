using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MemoToggle : MonoBehaviour
{
    [SerializeField] private GameObject memoBG;
    [SerializeField] private Sprite memoIcon;
    [SerializeField] private Sprite closeIcon;
    [SerializeField] private Image buttonImage;
    private float memoIconHeight = 611f;
    private float closeIconHeight = 574f;
    private float iconWidth = 574f;

    void Start()
    {
        memoBG.SetActive(false);
    }

    public void ToggleMemo()
    {
        memoBG.SetActive(!memoBG.activeSelf);
        if(memoBG.activeSelf)
        {
            buttonImage.sprite = closeIcon;
            Vector2 newTrans = new Vector2(iconWidth, closeIconHeight);
            buttonImage.rectTransform.sizeDelta = newTrans;
        }
        else
        {
            buttonImage.sprite = memoIcon;
            Vector2 newTrans = new Vector2(iconWidth, memoIconHeight);
            buttonImage.rectTransform.sizeDelta = newTrans;
        }
    }

}
