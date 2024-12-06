using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LogUnit
{
    private LogMenuController logMenuController;
    public TabType tabType;
    public bool isMyLog;
    public GameObject logObject;

    private CardsManager cardsManager;

    public static readonly int ElixerUsed = 0;
    public static readonly int OrbUsed = 1;
    public static readonly int OrbEffect = 2;
    public static readonly int MirrorUsed = 3;
    public static readonly int LensUsed = 4;
    public static readonly int LensEffect = 5;
    public static readonly int BalanceUsed = 6;
    public static readonly int ElixerEffectMore = 7;
    public static readonly int ElixerEffectLess = 8;
    public static readonly int AttackFailed = 9;
    public static readonly int Attacking = 10;
    public static readonly int NomalQuestion = 11;
    public static readonly int BalanceQuestion = 12;
    public static readonly int AttackLimited= 13;
    public static readonly int BalanceResult= 14;
    public static readonly int NomalResult= 15;
    public static readonly int ElixerEffectEqual = 16;
    public static readonly int ChainUsed = 17;


    public LogUnit(TabType tabType, bool isMyLog, int messageNum, int dataA = -1, int dataB = -1, int dataC = -1)
    {
        Debug.Log($"LogUnit初期化デバッグ: {messageNum} {dataA} {dataB} {dataC}");
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
            if(dataC >= 0)
            {
                Dictionary<int, string> logTexts = new Dictionary<int, string> {
                    {LensEffect, $"カード{CardsManager.intToAlph[dataA]}と{CardsManager.intToAlph[dataB]}の差は{dataC}!"},
                    {BalanceResult, $"カードの数字は {CardsManager.intToAlph[dataA]} < {CardsManager.intToAlph[dataB]} < {CardsManager.intToAlph[dataC]}"}
                };
                logTextTrans.GetComponent<TextMeshProUGUI>().text = logTexts[messageNum];
            }
            else if(dataB >= 0)
            {
                Dictionary<int, string> logTexts = new Dictionary<int, string> {
                    {OrbEffect, $"カード{CardsManager.intToAlph[dataA]}を{dataB}回動かした!"},
                    {ElixerEffectMore, $"カード{CardsManager.intToAlph[dataA]}の数字は{dataB}より大きい!"},
                    {ElixerEffectEqual, $"カード{CardsManager.intToAlph[dataA]}の数字は{dataB}と等しい!"},
                    {ElixerEffectLess, $"カード{CardsManager.intToAlph[dataA]}の数字は{dataB}より小さい!"},
                    {NomalResult, $"カードの数字は {CardsManager.intToAlph[dataA]} < {CardsManager.intToAlph[dataB]}"},
                };
                logTextTrans.GetComponent<TextMeshProUGUI>().text = logTexts[messageNum];
            }
            else
            {
                Dictionary<int, string> logTexts = new Dictionary<int, string> {
                    {ElixerUsed, $"{ItemUsingManager.itemNameDict[6]}を使用した!"},
                    {OrbUsed, $"{ItemUsingManager.itemNameDict[1]}を使用した!"},// 1
                    {MirrorUsed, $"{ItemUsingManager.itemNameDict[3]}を使用した!"},// 3
                    {LensUsed, $"{ItemUsingManager.itemNameDict[2]}を使用した!"},// 4
                    {BalanceUsed, $"{ItemUsingManager.itemNameDict[5]}を使用した!"},// 6
                    {AttackFailed, $"うまく決まらなかった!"},// 9
                    {Attacking, $"詠唱!"},// 10
                    {NomalQuestion, $"カードを2枚選択!"},// 11
                    {BalanceQuestion, $"カードを3枚選択!"},// 12
                    {AttackLimited, $"相手の{ItemUsingManager.itemNameDict[4]}によって詠唱できない!"},// 13
                    {ChainUsed, $"{ItemUsingManager.itemNameDict[4]}を使用した!"},// 3
                };
                logTextTrans.GetComponent<TextMeshProUGUI>().text = logTexts[messageNum];
            }
        }
    }
}