using System;
using UnityEngine;

public class PhaseManager : MonoBehaviour
{
    // フェーズごとの数字を変数で管理
    public const int InitialPhase = 0;
    public const int ItemPhase = 1;
    public const int QuestionPhase = 2;

    private ItemPhaseManager itemPhaseManager;
    private QutstionController qutstionController;

    private void Start()
    {
        itemPhaseManager = FindObjectOfType<ItemPhaseManager>();
        qutstionController = FindObjectOfType<QutstionController>();
    }
    

    // フェーズ変更時の処理
    public void HandlePhaseChange(int newPhase)
    {
        Debug.Log($"PhaseManager.HandlePhaseChangeが実行されました");

        switch (newPhase)
        {
            case InitialPhase:
                break;

            case ItemPhase:
                itemPhaseManager.StartItemPhase();
                break;

            case QuestionPhase:
                qutstionController.StartQuestionPhase();
                break;
        }
    }
}
