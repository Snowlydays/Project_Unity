using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;

public class PhaseManager : MonoBehaviour
{
    // フェーズごとの数字を変数で管理
    public const int InitialPhase = 0;
    public const int ItemPhase = 1;
    public const int QuestionPhase = 2;
    public const int ItemUsingPhase = 3;

    private NetworkSystem networkSystem;
    private LogMenuController logMenuController;

    public Sprite itemSprite;
    public Sprite questionSprite;

    GameObject canvas;

    public static int round = 0;

    private void Start()
    {
        networkSystem = FindObjectOfType<NetworkSystem>();
        logMenuController = FindObjectOfType<LogMenuController>();
        canvas = GameObject.Find("Canvas");
    }

    // フェーズ変更時の処理

    public void HandlePhaseChange(int newPhase){
        StartCoroutine(HandlePhaseChangeIEnumerator(newPhase));
    }
    
    IEnumerator HandlePhaseChangeIEnumerator(int newPhase)
    {
        Debug.Log($"PhaseManager.HandlePhaseChangeが実行されました");

        networkSystem.informationManager.ClearInformationText(); //infoをクリア
        logMenuController.CloseDrawer(); // フェイズが変わったらログタブを閉じる

        //ここで足なみを揃える
        while(networkSystem.hostWaiting!=0 || networkSystem.clientWaiting!=0){
            yield return new WaitForSeconds(0.33f);
        }

        while(networkSystem.hostWaiting!=1 || networkSystem.clientWaiting!=1){
            networkSystem.ChangeHostWaitingServerRPC(1);
            networkSystem.ChangeClientWaitingServerRPC(1);
            yield return new WaitForSeconds(0.33f);
        }

        networkSystem.ChangeHostWaitingServerRPC(0);
        networkSystem.ChangeClientWaitingServerRPC(0);

        switch (newPhase)
        {
            case InitialPhase:
                networkSystem.informationManager.ShowQuestionResult();
                networkSystem.mainSystemScript.readyButton.gameObject.GetComponent<Animator>().SetBool("blStarted", false);
                AttackAction();
                networkSystem.itemWindowManager.DisappearWindow();
                break;

            case ItemPhase:
                networkSystem.informationManager.questionResult = "";
                networkSystem.itemPhaseManager.StartItemPhase();
                networkSystem.animationController.CreatePhaseLogo(itemSprite);
                //networkSystem.mainSystemScript.readyButton.gameObject.GetComponent<Animator>().SetBool("blStarted", true);
                break;

            case QuestionPhase:
                networkSystem.questionController.StartQuestionPhase();
                networkSystem.animationController.CreatePhaseLogo(questionSprite);
                break;

            case ItemUsingPhase:
                networkSystem.itemUsingManager.StartItemUsePhase();
                networkSystem.itemWindowManager.AppearWindow();
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

    public static void ProgressRound()
    {
        round++;
        new LogUnit(TabType.All, true, -1, round);
        new LogUnit(TabType.Myself, true, -1, round);
        new LogUnit(TabType.Opponent, true, -1, round);
    }
}
