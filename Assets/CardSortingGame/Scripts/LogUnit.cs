using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/*
LogMenuContorollerに登録されているログUIの番号
0: エリクサー
1: オーブ 
2: オーブ効果 *
3: ミラー
4: レンズ
5: レンズ効果 *
6: 天秤
7: 天秤効果大 *
8: 天秤効果小 *
9: 失敗
10: 詠唱
11: 質問
12: (質問)エリクサー
13: 質問(鎖)
14: 質問結果(エリクサー) *
15: 質問結果(右)
16: 質問結果(左)
17: 鎖
*/

public class LogUnit
{
    private LogMenuController logMenuController;
    public TabType tabType;
    public bool isMyLog;
    public GameObject logObject;
    public Sprite logSprite;

    public LogUnit(TabType tabType, bool isMyLog, int messageNum, int dataA = -1, int dataB = -1, int dataC = -1)
    {
        if((tabType == TabType.Myself && !isMyLog) || (tabType == TabType.Opponent && isMyLog))
        {
            Debug.LogError("ログを追加しようとしているタブが違います");
        }

        logMenuController = Object.FindObjectOfType<LogMenuController>();
        this.tabType = tabType;
        
        // 使うスプライトとプレファブを決定
        Sprite sprite;
        GameObject prefab;
        if(isMyLog)
        {
            sprite = logMenuController.myLogSprites[messageNum];
            prefab = logMenuController.myLogPrefab;
        }
        else
        {
            sprite = logMenuController.opponentLogSprites[messageNum];
            prefab = logMenuController.opponentLogPrefab;
        }

        // 親を指定してクローン
        if(tabType == TabType.All) logObject = Object.Instantiate(prefab, logMenuController.allLogMenu.transform);
        else if(tabType == TabType.Myself) logObject = Object.Instantiate(prefab, logMenuController.myLogMenu.transform);
        else if(tabType == TabType.Opponent) logObject = Object.Instantiate(prefab, logMenuController.oppLogMenu.transform);
        Transform image = logObject.transform.Find("Image");
        image.GetComponent<Image>().sprite = sprite;

        // 結果により変化するテキストの処理
        if(messageNum == 2)
        {
            if(dataA < 0)
            {
                Debug.LogError("引数が正しくありません");
            }
            Transform text2 = image.transform.Find("Sprite2Text");
            text2.GetComponent<TextMeshProUGUI>().text = dataA.ToString();
        }
        else if(messageNum == 5)
        {
            if(dataA < 0)
            {
                Debug.LogError("引数が正しくありません");
            }
            Transform text5 = image.transform.Find("Sprite5Text");
            text5.GetComponent<TextMeshProUGUI>().text = dataA.ToString();
        }
        else if(messageNum == 7 || messageNum == 8)
        {
            if(dataA < 0)
            {
                Debug.LogError("引数が正しくありません");
            }
            Transform text78 = image.transform.Find("Sprite78Text");
            text78.GetComponent<TextMeshProUGUI>().text = dataA.ToString();
        }
        else if(messageNum == 14) // エリクサーを使う場合は引数で左から順に1~3の順序を割り振る
        {
            if(dataA <= 0 || dataB <= 0 || dataC <= 0)
            {
                return;
                // Debug.LogError("引数が正しくありません");
            }
            Transform text14L = image.transform.Find("Sprite14TextL");
            Transform text14M = image.transform.Find("Sprite14TextM");
            Transform text14R = image.transform.Find("Sprite14TextR");

            Transform[] text14 = {text14L, text14M, text14R};
            text14[dataA - 1].GetComponent<TextMeshProUGUI>().text = "L";
            text14[dataB - 1].GetComponent<TextMeshProUGUI>().text = "M";
            text14[dataC - 1].GetComponent<TextMeshProUGUI>().text = "R";
        }
    }
}