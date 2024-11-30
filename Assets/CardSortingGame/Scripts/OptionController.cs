using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.VisualScripting;
using TMPro;

public class OptionController : MonoBehaviour
{
    public GameObject openButton,backButton,endButton;

    GameObject noClickObject;

    private NetworkSystem networkSystem;
    
    float movedY;//オプションタブの移動先y座標を決める部分
    //ここの値を変えるだけでタブが滑らかに移動する
    
    void Start()
    {
        movedY=GetComponent<RectTransform>().anchoredPosition.y;
        openButton.GetComponent<Button>().onClick.AddListener(OnOptionButtonClicked);
        backButton.GetComponent<Button>().onClick.AddListener(OnBackButtonClicked);
        endButton.GetComponent<Button>().onClick.AddListener(OnEndButtonClicked);

        networkSystem = FindObjectOfType<NetworkSystem>();

        noClickObject=GameObject.Find("NoClickObject");

        noClickObject.SetActive(false);
    }

    void FixedUpdate()
    {
        float nowY=GetComponent<RectTransform>().anchoredPosition.y;
        GetComponent<RectTransform>().anchoredPosition+=new Vector2(
            0f,
            (movedY-nowY)/5f
        );
    }

    void OnOptionButtonClicked(){
        movedY=50f;
        noClickObject.SetActive(true);
    }

    void OnBackButtonClicked(){
        movedY=750f;
        noClickObject.SetActive(false);
    }

    void OnEndButtonClicked(){
       //降参処理
       //タイトルに戻るのスプライトを使う予定だったが、optionからゲーム画面に戻るボタンとごちゃつくと判断し
       //仮置きボタンで適応
       networkSystem.EndGame();
    }
}
