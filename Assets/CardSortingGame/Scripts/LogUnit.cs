using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogUnit : MonoBehaviour
{
    private LogMenuController logMenuController;
    public bool isMyLog;
    public GameObject logObject;
    public Sprite logSprite;

    /*
    0: エリクサー
    1: オーブ
    2: オーブ効果
    3: ミラー
    4: レンズ
    5: レンズ効果
    6: 天秤
    7: 天秤効果大
    8: 天秤効果小
    9: 失敗
    10: 詠唱
    11: 質問
    12: 質問(エリクサー)
    13: 質問(鎖)
    14: 質問結果(エリクサー)
    15: 質問結果(右)
    16: 質問結果(左)
    17: 鎖
    */

    public LogUnit(bool isMyLog, int messageNum)
    {
        this.isMyLog = isMyLog;
        if(isMyLog)
        {
            this.logSprite = logMenuController.myLogSprites[messageNum];
        }
    }

    void Start()
    {
        logMenuController = FindObjectOfType<LogMenuController>();
    }
}
