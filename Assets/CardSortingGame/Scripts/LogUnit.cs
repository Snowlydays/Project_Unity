using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogUnit
{
    private LogMenuController logMenuController;
    public TabType tabType;
    public bool isMyLog;
    public GameObject logObject;
    public Sprite logSprite;

    /*
    0: エリクサー *
    1: オーブ * 
    2: オーブ効果 
    3: ミラー *
    4: レンズ * 
    5: レンズ効果
    6: 天秤 *
    7: 天秤効果大 *
    8: 天秤効果小 *
    9: 失敗 *
    10: 詠唱 *
    11: 質問 *
    12: (質問)エリクサー *
    13: 質問(鎖) *
    14: 質問結果(エリクサー) *
    15: 質問結果(右) *
    16: 質問結果(左) *
    17: 鎖 *
    */

    public LogUnit(TabType tabType, bool isMyLog, int messageNum)
    {
        if((tabType == TabType.Myself && !isMyLog) || (tabType == TabType.Opponent && isMyLog))
        {
            Debug.LogError("ログを追加しようとしているタブが違います");
        }
        logMenuController = Object.FindObjectOfType<LogMenuController>();
        this.tabType = tabType;
        
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

        if(tabType == TabType.All)
        {
            logObject = Object.Instantiate(prefab, logMenuController.allLogMenu.transform);
            logObject.transform.Find("Image").GetComponent<Image>().sprite = sprite;
        }
        else if(tabType == TabType.Myself)
        {
            logObject = Object.Instantiate(prefab, logMenuController.myLogMenu.transform);
            logObject.transform.Find("Image").GetComponent<Image>().sprite = sprite;
        }
        else if(tabType == TabType.Opponent)
        {
            logObject = Object.Instantiate(prefab, logMenuController.oppLogMenu.transform);
            logObject.transform.Find("Image").GetComponent<Image>().sprite = sprite;
        }
    }

}
