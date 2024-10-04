using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QutstionController : MonoBehaviour
{
    GameObject bg;
    // GameObject[] mycardInfo = MainSystemScript.mycard;

    // Start is called before the first frame update
    void Start()
    {
        bg = GameObject.Find("QuestioningBG");

        // MainSystemScriptのmycard配列から情報を持ってきて、カードを選択できるUIとして使いたい
        // for(int i = 0; i < 7; i++)
        // {
        //     mycardInfo[i]=Instantiate(CardObject, new Vector3(1.5f*(float)(3-i),-0.75f,0.0f), Quaternion.identity);
        //     MainSystemScriptmycard[i].GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 1f);
        // } 
    }

    // phaseの値によってオブジェクトのActive状態を変更する関数
    void ManageActive()
    {
        if(NetworkSystem.phase == 2 && !bg.activeSelf)
        {
            bg.SetActive(true);
        }
        else if(NetworkSystem.phase != 2 && bg.activeSelf) 
        {
            bg.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        ManageActive();
    }
}
