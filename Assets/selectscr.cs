using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SelectScr : MonoBehaviour
{
    // Start is called before the first frame update
    public static int SortKind = 0;

    void OnMouseUp()
    {
        string SortName = this.gameObject.name;
        Debug.Log(SortName);
        if (SortName == "Bubble")SortKind = 1;
        else if (SortName == "Select")SortKind = 2;
        else if (SortName == "Insert")SortKind = 3;
        Debug.Log(SortKind);
    }
}