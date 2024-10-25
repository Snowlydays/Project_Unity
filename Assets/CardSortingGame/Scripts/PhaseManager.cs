using System;
using UnityEngine;

public class PhaseManager : MonoBehaviour
{
    // フェーズごとの数字を変数で管理
    public const int InitialPhase = 0;
    public const int ItemPhase = 1;
    public const int QuestionPhase = 2;

    private ItemPhaseManager itemPhaseManager;

    private void Start()
    {
        itemPhaseManager = FindObjectOfType<ItemPhaseManager>();
    }
    

    // フェーズ変更時の処理
    public void HandlePhaseChange(int newPhase)
    {
        Debug.Log($"フェーズが {newPhase} に変更されました。");

        switch (newPhase)
        {
            case InitialPhase:
                break;

            case ItemPhase:
                itemPhaseManager.StartItemPhase();
                break;

            case QuestionPhase:
                break;
        }
    }
}
