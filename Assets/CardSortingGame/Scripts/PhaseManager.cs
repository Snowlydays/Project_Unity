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
    private NetworkSystem networkSystem;

    private void Start()
    {
        itemPhaseManager = FindObjectOfType<ItemPhaseManager>();
        qutstionController = FindObjectOfType<QutstionController>();
        networkSystem = FindObjectOfType<NetworkSystem>();
    }
    

    // フェーズ変更時の処理
    public void HandlePhaseChange(int newPhase)
    {
        Debug.Log($"PhaseManager.HandlePhaseChangeが実行されました");

        switch (newPhase)
        {
            case InitialPhase:
                AttackAction();
                break;

            case ItemPhase:
                itemPhaseManager.StartItemPhase();
                break;

            case QuestionPhase:
                qutstionController.StartQuestionPhase();
                break;
        }
    }

    private void AttackAction()
    {
        bool hostAttacked = networkSystem.netIsHostAttacking.Value;
        bool clientAttacked = networkSystem.netIsClientAttacking.Value;

        if (hostAttacked || clientAttacked)
        {
            if (networkSystem.IsHost)
            {
                networkSystem.HandleAttackAction(hostAttacked,clientAttacked);
            }
        }
    }
}
