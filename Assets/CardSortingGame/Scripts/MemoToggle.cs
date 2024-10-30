using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoToggle : MonoBehaviour
{
    private GameObject memoCanvas;
    void Start()
    {
        memoCanvas = GameObject.Find("MemoCanvas");
        memoCanvas.SetActive(false);
    }

    public void ToggleMemo()
    {
        memoCanvas.SetActive(!memoCanvas.activeSelf);
    }

}
