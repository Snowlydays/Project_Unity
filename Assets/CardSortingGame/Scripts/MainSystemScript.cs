using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSystemScript : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]public GameObject CardObject;//カードのオブジェクト
    //オブジェクトはプレファブ化して単一で管理、操作したい場合は以下の配列で行う。
    //後々これをビリヤードよろしく自作クラスで管理できると上々



    GameObject[] mycard = new GameObject[7];//自分の手札
    GameObject[] othercard = new GameObject[7];//相手の手札
    void Start()
    {
        //開始時にカードオブジェクトのクローンを作成、配列で管理して並び替え等可能にしていきたい。
        for(int i = 0; i < 7; i++)
        {
            mycard[i]=Instantiate(CardObject, new Vector3(1.0f*(float)(3-i),-0.5f,0.0f), Quaternion.identity);
            mycard[i].GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 0f);
            othercard[i]=Instantiate(CardObject, new Vector3(1.0f*(float)(3-i),0.7f,0.0f), Quaternion.identity);
            othercard[i].GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //メインカメラを取得し、フェーズによってその背景色を変更する。
        //フェーズの取得はNetworkSystemが管理するphase変数から参照する。
        if(NetworkSystem.phase==2){
            Camera.main.backgroundColor = new Color(255f, 152f/255f, 226f/255f);
        }else{
            Camera.main.backgroundColor = new Color(1f, 1f, 1f);
        }
    }


    //networkvariableはアイテムが配られる時、アイテムを使用するとき、詠唱をするときにのみ使い、
    //それ以外の場面では使わない(使う必要がない)
}
