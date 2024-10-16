using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkSystem : NetworkBehaviour
{
    private NetworkVariable<int> netphase = new NetworkVariable<int>(0);

    public static int phase = 0;

    public override void OnNetworkSpawn()
    {
        netphase.OnValueChanged += (int oldParam, int newParam) =>
        {
            phase = newParam;
        };
    }

    public void changePhase(int phaseNum)
    {
        if (IsHost)
        {
            netphase.Value=phaseNum;
            Debug.Log("netphase.value変更");
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
