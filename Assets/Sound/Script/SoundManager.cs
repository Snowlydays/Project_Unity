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
        //cookieから音量設定を取得、なければデフォルト値にする
        float BGMvalue=Helper.GetCookieValue("BGM");
        if(BGMvalue==-1f){
            BGMvolume=0.5f;
        }else{
            BGMvolume=BGMvalue;
        }
        float SFXvalue=Helper.GetCookieValue("SFX");
        if(SFXvalue==-1f){
            SFXvolume=1f;
        }else{
            SFXvolume=SFXvalue;
        }
        ChangeBGMvolume(BGMvolume);
        ChangeSFXvolume(SFXvolume);
        nowPlayBGM=TitleBGM;
        myAudio.clip=nowPlayBGM;
        myAudio.Play();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void ChangeBGMvolume(float volume){
        //BGMの音量を変更するメソッド。同時にcookieに保存する
        BGMvolume=volume;
        myAudio.volume=BGMvolume;
        Helper.SetCookie("BGM",BGMvolume);
    }

    public void ChangeSFXvolume(float volume){
        //効果音全般の音量を変更するメソッド。同時にcookieに保存する
        SFXvolume=volume;
        PlaySound.volume=SFXvolume;
        Helper.SetCookie("SFX",SFXvolume);
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
