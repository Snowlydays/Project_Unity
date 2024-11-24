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

    public void AddInformationText(string str)
    {
        if(informationText.text.Length > 3 && informationText.text.Substring(informationText.text.Length - 3, 3) == "..." && str.Substring(str.Length - 3, 3) == "...") return;
        if(informationText.text != "") informationText.text = informationText.text += "\n";
        informationText.text = informationText.text + str;
    }

    public void ClearInformationText()
    {
        informationText.text = "";
    }

    public void ShowQuestionResult()
    {
        AddInformationText(questionResult);
    }
    
    public void SetQuestionResult(string str)
    {
        questionResult = str;
    }
}
