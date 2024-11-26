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
        if(informationText.text != "") informationText.text = informationText.text + "\n";
        informationText.text = informationText.text + str;

        //枠外に出るのを防止するためある程度改行していたら上から1行消す
        if(CountText("\n",informationText.text)>=3){
            int indend=informationText.text.IndexOf("\n");
            informationText.text=informationText.text.Remove(0,indend+1);
        }
    }

    public void ClearInformationText()
    {
        informationText.text = "";
    }

    public void ShowQuestionResult()
    {
        AddInformationText(questionResult);
    }
    
    public void AddQuestionResult(string str)
    {
        if(questionResult != "") questionResult = questionResult + "\n";
        questionResult = questionResult + str;
    }

    // 文字列から指定の文字が何回出てくるか調べるメソッド
    int CountText(string search,string target)
    {
        int cnt = 0;
        bool check = true;

        while (check)
        {
            if(target.IndexOf(search,System.StringComparison.CurrentCulture) == -1)
            {
                check = false;
            }
            else
            {
                target = target.Remove(0, target.IndexOf(search, System.StringComparison.CurrentCulture)+1);
                cnt++;
            }
        }

        return cnt;
    }
}
