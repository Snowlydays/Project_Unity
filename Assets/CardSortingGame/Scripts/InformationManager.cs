using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class InformationManager : MonoBehaviour
{
    public TextMeshProUGUI informationText;
    void Start()
    {
        informationText = this.GetComponent<TextMeshProUGUI>();
        ClearInformationText();
    }

    public void SetInformationText(string str)
    {
        informationText.text = str;

    }

    public void ClearInformationText()
    {
        informationText.text = "";
    }
}
