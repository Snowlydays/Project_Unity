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

    public GameObject BGMSlider,SFXSlider;

    GameObject noClickObject;

    private NetworkSystem networkSystem;
    
    float movedY;

    SoundManager soundManager;
    
    void Start()
    {
        movedY=GetComponent<RectTransform>().anchoredPosition.y;
        openButton.GetComponent<Button>().onClick.AddListener(OnOptionButtonClicked);
        backButton.GetComponent<Button>().onClick.AddListener(OnBackButtonClicked);
        endButton.GetComponent<Button>().onClick.AddListener(OnEndButtonClicked);

        soundManager=FindObjectOfType<SoundManager>();

        BGMSlider.GetComponent<Slider>().value=SoundManager.BGMvolume;
        SFXSlider.GetComponent<Slider>().value=SoundManager.SFXvolume;

        BGMSlider.GetComponent<Slider>().onValueChanged.AddListener(OnBGMSlide);
        SFXSlider.GetComponent<Slider>().onValueChanged.AddListener(OnSFXSlide);

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
    
    void OnBGMSlide(float value){
        soundManager.ChangeBGMvolume(value);
    }

    void OnSFXSlide(float value){
        soundManager.ChangeSFXvolume(value);
    }
}