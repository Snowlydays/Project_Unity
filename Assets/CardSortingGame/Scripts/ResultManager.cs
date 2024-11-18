using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class ResultManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("BackButton").GetComponent<Button>().onClick.AddListener(backtitle);
        AuthenticationService.Instance.SignOut();
    }

    // Update is called once per frame
    void Update()
    {
        //networkmanagerが存在したら常に破棄
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }
    }

    void backtitle(){
        Debug.Log("タイトルに戻りました");
        SceneManager.LoadScene("StartScene");
    }
}
