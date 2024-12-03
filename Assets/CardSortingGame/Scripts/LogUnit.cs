using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/*
ログテキストの番号(LogMenuContorollerに登録されていたログUIの番号を引き継ぎ)
例外としてラウンド表示は-1
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

    private CardsManager cardsManager;

    public LogUnit(TabType tabType, bool isMyLog, int messageNum, int dataA = -1, int dataB = -1, int dataC = -1)
    {
        Debug.Log($"{messageNum} {dataA} {dataB} {dataC}");
        if((tabType == TabType.Myself && !isMyLog) || (tabType == TabType.Opponent && isMyLog))
        {
            if(messageNum > 0) Debug.LogError("ログを追加しようとしているタブが違います");
        }

        logMenuController = Object.FindObjectOfType<LogMenuController>();
        this.tabType = tabType;
        
        // 使うスプライトとプレファブを決定
        GameObject prefab;
        if(messageNum == -1)
        {
            prefab = logMenuController.roundLogPrefab;
        }
        else if(isMyLog)
        {
            prefab = logMenuController.myLogPrefab;
        }
        else
        {
            prefab = logMenuController.opponentLogPrefab;
        }

        // 親を指定してクローン
        if(tabType == TabType.All) this.logObject = Object.Instantiate(prefab, logMenuController.allLogMenu.transform);
        else if(tabType == TabType.Myself) this.logObject = Object.Instantiate(prefab, logMenuController.myLogMenu.transform);
        else if(tabType == TabType.Opponent) this.logObject = Object.Instantiate(prefab, logMenuController.oppLogMenu.transform);
        Transform image = this.logObject.transform.Find("Image");
        if(messageNum == -1)
        {
            if(dataA < 1)
            {
                Debug.LogError("引数が正しくありません");
                return;
            }
            Transform roundNumText = image.transform.Find("RoundNumText");
            roundNumText.GetComponent<TextMeshProUGUI>().text = dataA.ToString(); 
        }
        else
        {
            Transform logTextTrans = image.transform.Find("LogText");
            cardsManager = Object.FindObjectOfType<CardsManager>();
            if(messageNum == 5)
            {
                logTextTrans.GetComponent<TextMeshProUGUI>().text = $"カード{CardsManager.intToAlph[dataA]}と{CardsManager.intToAlph[dataB]}の差は{dataC}!";
            }
            else if(messageNum == 7)
            {
                logTextTrans.GetComponent<TextMeshProUGUI>().text = $"カード{CardsManager.intToAlph[dataA]}は{dataB + 1}以上!";
            }
            else if(messageNum == 8)
            {
                logTextTrans.GetComponent<TextMeshProUGUI>().text = $"カード{CardsManager.intToAlph[dataA]}は{dataB}以下!";
            }
            else if(messageNum == 14)
            {
                logTextTrans.GetComponent<TextMeshProUGUI>().text = $"{CardsManager.intToAlph[dataA]} > {CardsManager.intToAlph[dataB]} > {CardsManager.intToAlph[dataC]}の順番に大きい";
            }
            else if(messageNum == 15)
            {
                logTextTrans.GetComponent<TextMeshProUGUI>().text = $"カード{CardsManager.intToAlph[dataA]}よりカード{CardsManager.intToAlph[dataB]}の方が大きい";
            }
            else
            {
                string[] logTexts = {
                    $"{ItemUsingManager.itemNameDict[6]}を使用した!",// 0
                    $"{ItemUsingManager.itemNameDict[1]}を使用した!",// 1
                    $"カードを{dataA}回動かした!",// 2
                    $"{ItemUsingManager.itemNameDict[3]}を使用した!",// 3
                    $"{ItemUsingManager.itemNameDict[2]}を使用した!",// 4
                    $"",// 5
                    $"{ItemUsingManager.itemNameDict[5]}を使用した!",// 6
                    $"",// 7
                    $"",// 8
                    $"うまく決まらなかった!",// 9
                    $"詠唱!",// 10
                    $"カードを2枚選択!",// 11
                    $"カードを3枚選択!",// 12
                    $"{ItemUsingManager.itemNameDict[4]}によって質問できない!",// 13
                    $"",// 14
                    $"",// 15
                    $"左のカードの数字の方が大きい",// 16
                    $"{ItemUsingManager.itemNameDict[4]}を使用した!",// 3
                };
                logTextTrans.GetComponent<TextMeshProUGUI>().text = logTexts[messageNum];
            }
        }
    }
}