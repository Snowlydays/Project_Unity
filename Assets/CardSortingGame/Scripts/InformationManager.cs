using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class InformationManager : MonoBehaviour
{
    public TextMeshProUGUI informationText;
    public string questionResult = "";
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

    public void ShowQuestionResult()
    {
        SetInformationText(questionResult);
    }
}
