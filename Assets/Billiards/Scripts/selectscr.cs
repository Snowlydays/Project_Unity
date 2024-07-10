using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SelectScr : MonoBehaviour
{
    public static int SortKind = 0;

    void OnMouseUp()
    {
        // ソート実行中でないときのみ変更を受け付け
        if (!SortController.isSorging && !WhiteBall.isMoving)
        {
            string SortName = this.gameObject.name;
            if (SortName == "Bubble") SortKind = 1;
            else if (SortName == "Select") SortKind = 2;
            else if (SortName == "Insert") SortKind = 3;
            Debug.Log(SortName + " " + SortKind);
        }
    }
}