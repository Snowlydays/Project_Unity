using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhaseManager : MonoBehaviour
{
    // フェーズごとの数字を変数で管理
    public const int InitialPhase = 0;
    public const int ItemPhase = 1;
    public const int QuestionPhase = 2;
    public const int ItemUsingPhase = 3;

    private NetworkSystem networkSystem;

    private void Start()
    {
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
                networkSystem.itemPhaseManager.StartItemPhase();
                break;

            case QuestionPhase:
                networkSystem.qutstionController.StartQuestionPhase();
                break;

            case ItemUsingPhase:
                networkSystem.itemUsingManager.StartItemUsePhase();
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
            
            // 攻撃トグルのリセット
            networkSystem.ToggleAttackedReset();
        }
    }
}
