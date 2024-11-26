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

    public Sprite[] itemSprites;

    public GameObject itemThreeMyObject;

    public GameObject itemThreeOtherObject;

    public GameObject itemDefaultObject;

    public GameObject itemDefaultOtherObject;

    public Sprite newSprite;

    void Start(){
        networkSystem = FindObjectOfType<NetworkSystem>(); 
    }

    public void OnPhaseAnimationEnd()
    {
        //フェーズ開始時のロゴアニメーション終了時の処理
        //他にもアニメーション終了系全般はこれ
        //自身を削除してシーン内にオブジェクトが溜まるのを防止
        Destroy(this.gameObject);
    }

    public void OnChangeThreeItem(){
        //スプライトを変更
        this.GetComponent<Image>().overrideSprite=newSprite;
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

    public void CreateMyThreeItem(int newitem,float posx=0f){
        GameObject canvas=networkSystem.itemUsingManager.itemUseCanvas;
        animobj=Instantiate(itemThreeMyObject);
        animobj.transform.SetParent(canvas.transform);
        animobj.GetComponent<RectTransform>().anchoredPosition=new Vector3(posx,0f,0f);
        animobj.GetComponent<AnimationController>().newSprite=itemSprites[newitem];
    }

    public void CreateOtherThreeItem(int newitem,float posx=0f){
        GameObject canvas=networkSystem.itemUsingManager.itemUseCanvas;
        animobj=Instantiate(itemThreeOtherObject);
        animobj.transform.SetParent(canvas.transform);
        animobj.GetComponent<RectTransform>().anchoredPosition=new Vector3(posx,0f,0f);
        animobj.transform.localScale=new Vector3(1f,-1f,1f);
        animobj.GetComponent<AnimationController>().newSprite=itemSprites[newitem];
    }

    public void CreateDefaultItem(int newitem,float posx=0f){
        GameObject canvas=networkSystem.itemUsingManager.itemUseCanvas;
        animobj=Instantiate(itemDefaultObject);
        animobj.transform.SetParent(canvas.transform);
        animobj.GetComponent<RectTransform>().anchoredPosition=new Vector3(posx,0f,0f);
        animobj.GetComponent<Image>().overrideSprite=itemSprites[newitem];
    }

    public void CreateDefaultOtherItem(int newitem,float posx=0f){
        GameObject canvas=networkSystem.itemUsingManager.itemUseCanvas;
        animobj=Instantiate(itemDefaultOtherObject);
        animobj.transform.SetParent(canvas.transform);
        animobj.GetComponent<RectTransform>().anchoredPosition=new Vector3(posx,0f,0f);
        animobj.transform.localScale=new Vector3(1f,-1f,1f);
        animobj.GetComponent<Image>().overrideSprite=itemSprites[newitem];
    }
}
