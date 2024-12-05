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

    public const int ElixerUsed = 0;
    public const int OrbUsed = 1;
    public const int OrbEffect = 2;
    public const int MirrorUsed = 3;
    public const int LensUsed = 4;
    public const int LensEffect = 5;
    public const int BalanceUsed = 6;
    public const int ElixerEffectBig = 7;
    public const int ElixerEffectSmall = 8;
    public const int AttackFailed = 9;
    public const int Attacking = 10;
    public const int NomalQuestion = 11;
    public const int BalanceQuestion = 12;
    public const int AttackLimited= 13;
    public const int BalanceResult= 14;
    public const int NomalResult= 15;
    public const int ChainUsed = 17;


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
            if(messageNum == LensEffect)
            {
                logTextTrans.GetComponent<TextMeshProUGUI>().text = $"カード{CardsManager.intToAlph[dataA]}と{CardsManager.intToAlph[dataB]}の差は{dataC}!";
            }
            else if(messageNum == ElixerEffectBig)
            {
                logTextTrans.GetComponent<TextMeshProUGUI>().text = $"カード{CardsManager.intToAlph[dataA]}は{dataB + 1}以上!";
            }
            else if(messageNum == ElixerEffectSmall)
            {
                logTextTrans.GetComponent<TextMeshProUGUI>().text = $"カード{CardsManager.intToAlph[dataA]}は{dataB}以下!";
            }
            else if(messageNum == BalanceResult)
            {
                logTextTrans.GetComponent<TextMeshProUGUI>().text = $"{CardsManager.intToAlph[dataA]} > {CardsManager.intToAlph[dataB]} > {CardsManager.intToAlph[dataC]}の順番に大きい";
            }
            else if(messageNum == NomalResult)
            {
                logTextTrans.GetComponent<TextMeshProUGUI>().text = $"カード{CardsManager.intToAlph[dataA]}よりカード{CardsManager.intToAlph[dataB]}の方が大きい";
            }
            else
            {
                Dictionary<int, string> logTexts = new Dictionary<int, string> {
                    {ElixerUsed, $"{ItemUsingManager.itemNameDict[6]}を使用した!"},
                    {OrbUsed, $"{ItemUsingManager.itemNameDict[1]}を使用した!"},// 1
                    {OrbEffect, $"カードを{dataA}回動かした!"},// 2
                    {MirrorUsed, $"{ItemUsingManager.itemNameDict[3]}を使用した!"},// 3
                    {LensUsed, $"{ItemUsingManager.itemNameDict[2]}を使用した!"},// 4
                    {BalanceUsed, $"{ItemUsingManager.itemNameDict[5]}を使用した!"},// 6
                    {AttackFailed, $"うまく決まらなかった!"},// 9
                    {Attacking, $"詠唱!"},// 10
                    {NomalQuestion, $"カードを2枚選択!"},// 11
                    {BalanceQuestion, $"カードを3枚選択!"},// 12
                    {AttackLimited, $"{ItemUsingManager.itemNameDict[4]}によって質問できない!"},// 13
                    {ChainUsed, $"{ItemUsingManager.itemNameDict[4]}を使用した!"},// 3
                };
                logTextTrans.GetComponent<TextMeshProUGUI>().text = logTexts[messageNum];
            }
        }
    }
}