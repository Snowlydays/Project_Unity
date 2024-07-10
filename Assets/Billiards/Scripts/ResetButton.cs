using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ResetButton : MonoBehaviour
{
    public void OnClick()
    {
        SortController.ShuffleBalls(SortController.myBall);
        SelectScr.SortKind = 0;
        // フォーカス状態を外す
        EventSystem.current.SetSelectedGameObject(null);
    }
}