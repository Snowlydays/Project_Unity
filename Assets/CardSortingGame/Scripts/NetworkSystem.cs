using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkSystem : NetworkBehaviour
{
    // フェーズごとの数字を変数で管理
    const int initialPhase = 0;
    const int itemPhase = 1;
    const int questionPhase = 2;

    private ItemPhaseManager itemPhaseManager;

    // フェーズを管理するNetWrokVariable
    private NetworkVariable<int> netphase = new NetworkVariable<int>(0);

    // プレイヤーのReady状態を管理するNetworkVariables
    private NetworkVariable<bool> netHostReady = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> netClientReady = new NetworkVariable<bool>(false);

    public static int phase = 0;
    public static bool hostReady = false;
    public static bool clientReady = false;
    
    private PhaseManager phaseManager;

    private void Awake()
    {
        phaseManager = FindObjectOfType<PhaseManager>();
    }

    public override void OnNetworkSpawn()
    {
        netphase.OnValueChanged += (int oldParam, int newParam) =>
        {
            phase = newParam;
            Debug.Log($"フェーズが {newParam} になりました。");
            phaseManager.HandlePhaseChange(newParam);
        };

        netHostReady.OnValueChanged += (bool oldParam, bool newParam) =>
        {
            hostReady = newParam;
            Debug.Log($"hostのReady状態が {newParam} になりました。");
            CheckAllPlayersReady();
        };

        netClientReady.OnValueChanged += (bool oldParam, bool newParam) =>
        {
            clientReady = newParam;
            Debug.Log($"clientのReady状態が {newParam} になりました。");
            CheckAllPlayersReady();
        };
    }

    public void ChangePhase(int nextPhase)
    {
        if (IsHost)
        {
            netphase.Value = nextPhase;
        }
    }
    
    // すべてのプレイヤーがReadyかをチェックし、フェーズを進める
    public void CheckAllPlayersReady()
    {
        if (hostReady && clientReady)
        {
            switch (phase)
            {
                case initialPhase:
                    ChangePhase(itemPhase);
                    break;

                case itemPhase:
                    ChangePhase(questionPhase);
                    break;

                case questionPhase:
                    ChangePhase(initialPhase);
                    break;
            }
            ResetReadyStates();
        }
    }

    // プレイヤーのReadyフラグを初期化
    private void ResetReadyStates()
    {
        hostReady = false;
        clientReady = false;
        netHostReady.Value = false;
        netClientReady.Value = false;
    }

    public void ToggleReady()
    {
        if (IsHost)
        {
            netHostReady.Value = !netHostReady.Value;
        }
        else
        {
            ClientReadyChange();
        }
    }

    void Update()
    {
        if (IsHost)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                netphase.Value = 0;
                Debug.Log("host changed phase 0");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                netphase.Value = 1;
                Debug.Log("host changed phase 1");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                netphase.Value = 2;
                Debug.Log("host changed phase 2");
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                ClientPhaseChange(0);
                Debug.Log("client changed phase 0");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ClientPhaseChange(1);
                Debug.Log("client changed phase 1");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ClientPhaseChange(2);
                Debug.Log("client changed phase 2");
            }
        }
    }

    void ClientReadyChange()
    {
        ClientReadyChangeServerRpc();
    }

    [Unity.Netcode.ServerRpc(RequireOwnership = false)]
    void ClientReadyChangeServerRpc()
    {
        netClientReady.Value = !netClientReady.Value;
    }

    void ClientPhaseChange(int num)
    {
        ClientPhaseChangeServerRpc(num);
    }

    [Unity.Netcode.ServerRpc(RequireOwnership = false)]
    void ClientPhaseChangeServerRpc(int num)
    {
        netphase.Value = num;
    }
}