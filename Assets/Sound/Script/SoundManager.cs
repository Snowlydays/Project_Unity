using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public AudioClip TitleBGM;
    public AudioClip GameBGM;

    AudioClip nowPlayBGM;

    AudioSource myAudio;

    public static float BGMvolume;//BGMの音量
    public static float SFXvolume;//効果音の音量

    void Start()
    {
        myAudio=GetComponent<AudioSource>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        nowPlayBGM=TitleBGM;
        myAudio.clip=nowPlayBGM;
        myAudio.Play();
        //何らかの手段(例えばcookieなど)で音量設定を保存できるとなお良いかも
        BGMvolume=0.5f;
        SFXvolume=1f;
        ChangeBGMvolume(BGMvolume);
        ChangeSFXvolume(SFXvolume);
    }

    public void ChangeBGMvolume(float volume){
        //BGMの音量を変更するメソッド
        BGMvolume=volume;
        myAudio.volume=BGMvolume;
    }

    public void ChangeSFXvolume(float volume){
        //効果音全般の音量を変更するメソッド
        SFXvolume=volume;
        PlaySound.volume=SFXvolume;
    }

    void OnSceneLoaded( Scene scene, LoadSceneMode mode )
    {
        //シーンがsingleで読み込まれる時のみ実行、Addでは実行されない。
        if(mode.ToString()=="Additive")return;
        //シーンごとに適したBGMを選択
        switch(scene.name){
            case "CardSortingGame":
                nowPlayBGM=GameBGM;
            break;

            case "WinScene": case "LoseScene": case "DrawScene":
                nowPlayBGM=null;
            break;

            default:
                nowPlayBGM=TitleBGM;
            break;
        }
        if(nowPlayBGM!=null){
            //現在流れているBGMと選択されたBGMが違った場合は選択されたBGMにかけなおす
            if(myAudio.clip != nowPlayBGM){
                myAudio.clip=nowPlayBGM;
                myAudio.Play();
            }
        }else{
            //nullなら無音
            myAudio.Stop();
        }
    }

}
