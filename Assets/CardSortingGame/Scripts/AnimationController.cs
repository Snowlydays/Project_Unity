using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimationController : MonoBehaviour
{
    //何らかのアニメーションをするオブジェクト全てを総括的に処理するスクリプト

    private NetworkSystem networkSystem;

    public GameObject phaseAnimObject;

    public GameObject animobj;

    public AudioClip logoSound;

    public GameObject logoSoundObject;

    void Start(){
        networkSystem = FindObjectOfType<NetworkSystem>(); 
    }

    public void OnPhaseAnimationEnd()
    {
        //フェーズ開始時のロゴアニメーション終了時の処理
        //自身を削除してシーン内にオブジェクトが溜まるのを防止
        Destroy(this.gameObject);
    }

    public void CreatePhaseLogo(Sprite sprite){
        GameObject soundobj=Instantiate(logoSoundObject);
        soundobj.GetComponent<PlaySound>().PlaySE(logoSound);
        GameObject canvas=networkSystem.itemUsingManager.itemUseCanvas;
        animobj=Instantiate(phaseAnimObject);
        animobj.transform.SetParent(canvas.transform);
        animobj.GetComponent<RectTransform>().anchoredPosition=Vector3.zero;
        animobj.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 29f);
        animobj.GetComponentsInChildren<Image>()[1].overrideSprite=sprite;
    }
}
