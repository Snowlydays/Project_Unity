using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ManagerUIscript : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)){
            NetworkManager.Singleton.StartHost();
        }
        if(Input.GetKeyDown(KeyCode.Backspace)){
            NetworkManager.Singleton.StartClient();
        }
    }
}