using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkSystem : NetworkBehaviour
{
    private NetworkVariable<int> netphase = new NetworkVariable<int>(0);

    private NetworkVariable<bool> netHostReady = new NetworkVariable<bool>(false);

    private NetworkVariable<bool> netClientReady = new NetworkVariable<bool>(false);

    public static int phase = 0;

    public static bool hostReady = false;

    public static bool clientReady = false;

    public override void OnNetworkSpawn()
    {
        netphase.OnValueChanged += (int oldParam, int newParam) =>
        {
            phase = newParam;
        };

        netHostReady.OnValueChanged += (bool oldParam, bool newParam) =>
        {
            hostReady = newParam;
        };

        netClientReady.OnValueChanged += (bool oldParam, bool newParam) =>
        {
            clientReady = newParam;
        };
    }

    public void ChangePhase(int phaseNum)
    {
        if (IsHost)
        {
            netphase.Value=phaseNum;
            Debug.Log("netphase.value変更");
        }
    }

    public void ToggleReady(){
        if (IsHost){
            netHostReady.Value=!netHostReady.Value;
            Debug.Log("Host changed ready");
            Debug.Log(netHostReady.Value);
        }else{
            ClientReadyChange();
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        if(IsHost){
            if(Input.GetKeyDown(KeyCode.Alpha0)){
                netphase.Value=0;
                Debug.Log("host changed phase 0");
            }else if(Input.GetKeyDown(KeyCode.Alpha1)){
                netphase.Value=1;
                Debug.Log("host changed phase 1");
            }else if(Input.GetKeyDown(KeyCode.Alpha2)){
                netphase.Value=2;
                Debug.Log("host changed phase 2");
            }
        }else{
            if(Input.GetKeyDown(KeyCode.Alpha0)){
                ClientPhaseChange(0);
                Debug.Log("client changed phase 0");
            }else if(Input.GetKeyDown(KeyCode.Alpha1)){
                ClientPhaseChange(1);
                Debug.Log("client changed phase 1");
            }else if(Input.GetKeyDown(KeyCode.Alpha2)){
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
        Debug.Log("Client changed ready");
        Debug.Log(netClientReady.Value);
    }
    
    void ClientPhaseChange(int num)
    {
        ClientPhaseChangeServerRpc(num);
    }

    [Unity.Netcode.ServerRpc(RequireOwnership = false)]
    void ClientPhaseChangeServerRpc(int num)
    {
        netphase.Value=num;
    }
}
