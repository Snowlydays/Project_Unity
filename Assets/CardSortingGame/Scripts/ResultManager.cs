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
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip drawSound;
    public AudioClip decideSound;
    public GameObject SoundObject;

    void Start()
    {
        GameObject.Find("BackButton").GetComponent<Button>().onClick.AddListener(backtitle);
        AuthenticationService.Instance.SignOut();

        string scenename = SceneManager.GetActiveScene().name;

        Debug.Log(scenename);

        GameObject soundobj;

        switch(scenename){
            case "WinScene":
            soundobj=Instantiate(SoundObject);
            soundobj.GetComponent<PlaySound>().PlaySE(winSound);
            break;
            case "LoseScene":
            soundobj=Instantiate(SoundObject);
            soundobj.GetComponent<PlaySound>().PlaySE(loseSound);
            break;
            case "DrawScene":
            soundobj=Instantiate(SoundObject);
            soundobj.GetComponent<PlaySound>().PlaySE(drawSound);
            break;
        }
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
        GameObject soundobj=Instantiate(SoundObject);
        soundobj.GetComponent<PlaySound>().PlaySE(decideSound);
        SceneManager.LoadScene("StartScene");
    }
}
