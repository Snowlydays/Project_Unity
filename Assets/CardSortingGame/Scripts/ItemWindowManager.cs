using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemWindowManager : MonoBehaviour
{
    float movedX;

    RectTransform myrect;

    public GameObject iconObject;

    GameObject[] iconObjects;

    public Sprite[] itemSprites;

    void Start()
    {
        myrect=GetComponent<RectTransform>();
        movedX=myrect.anchoredPosition.x;

    }

    public void SetWindowIcons(int[] array){
        int itemNum=array.Length;
        if(iconObjects?.Length>0){
            for(int i=0;i<iconObjects.Length;i++){
                Destroy(iconObjects[i]);
            }
        }
        iconObjects=new GameObject[itemNum];
        if(itemNum<=0){
            movedX=720f;
            return;
        }
        float startY=60f*(float)(itemNum-1);
        myrect.sizeDelta=new Vector2(120f,120f*(float)itemNum);
        this.transform.localScale=new Vector3(1f,1f,1f);
        for(int i=0;i<itemNum;i++){
            iconObjects[i]=Instantiate(iconObject);
            iconObjects[i].transform.SetParent(this.transform);
            iconObjects[i].GetComponent<Image>().sprite=itemSprites[array[i]];
            iconObjects[i].GetComponent<RectTransform>().anchoredPosition=new Vector3(0f,startY-120f*(float)i,0f);
            iconObjects[i].transform.localScale=new Vector3(1f,1f,1f);
        }
    }

    public void AppearWindow(){
        if(iconObjects?.Length>0)movedX=560;
    }
    public void DisappearWindow(){
        movedX=720f;
    }

    void FixedUpdate()
    {
        float nowX=myrect.anchoredPosition.x;
        myrect.anchoredPosition+=new Vector2(
            (movedX-nowX)/5f,
            0f
        );
    }
}
