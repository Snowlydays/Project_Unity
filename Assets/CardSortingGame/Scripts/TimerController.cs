using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEditor;
using Unity.Collections;
using Random = UnityEngine.Random;

public class TimerController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject circleObject,readyButton; // 制限時間バー
    Image circleImage,buttonImage;
    public bool isCountNow=false;
    float H,S,V,nowTime,buttonX,buttonY;
    public float timerSec=60f;//計測する制限時間(秒)
    [SerializeField] private Sprite CountingSprite;
    [SerializeField] private Sprite notCountingSprite;

    NetworkSystem networkSystem;

    void Start()
    {
        networkSystem = FindObjectOfType<NetworkSystem>();
        buttonImage = readyButton.GetComponent<Image>();
        buttonX=readyButton.GetComponent<RectTransform>().anchoredPosition.x;
        buttonY=readyButton.GetComponent<RectTransform>().anchoredPosition.y;
        circleImage=circleObject.GetComponent<Image>();
        circleObject.SetActive(false);
        Color.RGBToHSV(circleImage.color, out H, out S, out V);
        StartDrawBar();
    }

    void FixedUpdate(){
        if(isCountNow){
            float timeScale=nowTime/timerSec;
            circleImage.color=Color.HSVToRGB(H*timeScale, S, V);
            circleImage.fillAmount=timeScale;
            readyButton.GetComponent<RectTransform>().anchoredPosition=new Vector3(
                buttonX+Random.Range(0, 10)*Mathf.Max(0f,(0.25f-timeScale)*4f),
                buttonY+Random.Range(0, 10)*Mathf.Max(0f,(0.25f-timeScale)*4f),
                0f
            );
            nowTime-=1f/50f;
            if(nowTime<=0f){
                networkSystem.mainSystemScript.OnReadyButtonClicked();
                isCountNow=false;
            }
        }
    }

    public void StartDrawBar(){
        isCountNow=true;
        nowTime=timerSec;
        circleObject.SetActive(true);
        buttonImage.sprite=CountingSprite;
        readyButton.GetComponent<Button>().enabled=true;
    }

    public void EndDrawBar(){
        isCountNow=false;
        nowTime=0f;
        buttonImage.sprite=notCountingSprite;
        circleObject.SetActive(false);
        readyButton.GetComponent<RectTransform>().anchoredPosition=new Vector3(buttonX,buttonY,0f);
        readyButton.GetComponent<Button>().enabled=false;
        
    }
}
