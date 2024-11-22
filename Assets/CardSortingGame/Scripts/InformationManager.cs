using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class InformationManager : MonoBehaviour
{
    public TextMeshProUGUI informationText;
    public string questionResult = "";
    public bool isTextAdded = false;

    void Start()
    {
        informationText = this.GetComponent<TextMeshProUGUI>();
        ClearInformationText();
    }

    public void AddInformationText(string str)
    {
        if(isTextAdded)
        {
            isTextAdded = false;
            return;
        }
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
}
