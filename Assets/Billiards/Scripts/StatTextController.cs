using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatTextController : MonoBehaviour
{
    public TextMeshProUGUI statText;
    void Update()
    {
        int myBallLen = SortController.myBall.Length;
        if (SortController.current_idx >= myBallLen - 1) // ソート完了を表示する
        {
            statText.text = "Sort Completed"; 
        }
        else if(SortController.isSorging) // ソート中は直前に選択されたものを表示
        {
            if(WhiteBall.SortKind == 1)statText.text = "Bubble Sort";
            else if(WhiteBall.SortKind == 2)statText.text = "Selection Sort";
            else if(WhiteBall.SortKind == 3)statText.text = "Insertion Sort";
            else statText.text = "";
        }
        else // それ以外は内部の選択状態を表示
        {
            if(SelectScr.SortKind == 1)statText.text = "Bubble Sort";
            else if(SelectScr.SortKind == 2)statText.text = "Selection Sort";
            else if(SelectScr.SortKind == 3)statText.text = "Insertion Sort";
            else statText.text = "";
        }
    } 
}

