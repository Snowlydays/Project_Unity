using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;

public class PhaseManager : MonoBehaviour
{
    // フェーズごとの数字を変数で管理
    public const int InitialPhase = 0;
    public const int ItemPhase = 1;
    public const int QuestionPhase = 2;
    public const int ItemUsingPhase = 3;

    private NetworkSystem networkSystem;

    public GameObject phaseAnimObject;
    public Sprite questionSprite;

    GameObject canvas;

    private void Start()
    {
        networkSystem = FindObjectOfType<NetworkSystem>();
        canvas = GameObject.Find("Canvas");
    }

    // フェーズ変更時の処理

    public void HandlePhaseChange(int newPhase){
        StartCoroutine(HandlePhaseChangeIEnumerator(newPhase));
    }
    
    IEnumerator HandlePhaseChangeIEnumerator(int newPhase)
    {
        Debug.Log($"PhaseManager.HandlePhaseChangeが実行されました");

        networkSystem.informationManager.ClearInformationText();

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

        GameObject animobj;
        switch (newPhase)
        {
            case InitialPhase:
                networkSystem.informationManager.ShowQuestionResult();
                AttackAction();
                break;

            case ItemPhase:
                networkSystem.itemPhaseManager.StartItemPhase();
                //animobj=Instantiate(phaseAnimObject,new Vector3(0f,0f,0f),Quaternion.identity);
                //animobj.transform.SetParent(canvas.transform);
                break;

            case QuestionPhase:
                networkSystem.questionController.StartQuestionPhase();
                //animobj=Instantiate(phaseAnimObject,new Vector3(0f,0f,0f),Quaternion.identity);
                //animobj.transform.SetParent(canvas.transform);
                //animobj.transform.GetComponent<Image>().sprite=questionSprite;
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
