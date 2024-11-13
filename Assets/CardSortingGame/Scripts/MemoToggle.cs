using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MemoToggle : MonoBehaviour
{
    private GameObject memoBG;
    private TextMeshProUGUI buttonText;
    void Start()
    {
        memoBG = GameObject.Find("MemoBG");
        memoBG.SetActive(false);
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void ToggleMemo()
    {
        memoBG.SetActive(!memoBG.activeSelf);
        if(memoBG.activeSelf)
        {
            // buttonText.text = "x";
        }
        else
        {
            // buttonText.text = "memo";
        }
    }

}
