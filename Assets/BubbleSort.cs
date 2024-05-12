using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BubbleSort : MonoBehaviour
{
    BallClass[] myball = new BallClass[5];

    void Start()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("ball");
        
        // ボール番号を名前から整数で取得
        // BallClassとして取得
        for(int i=0;i<5;i++){
            int ballnum = int.Parse(objects[i].name.Substring(5));
            myball[i] = new BallClass(objects[i],ballnum);
        }
    }

    void Update()
    {
        // ボールの動きをBallClassの情報と連動させる
        for(int i=0;i<5;i++){
            Transform balltrans = myball[i].ballobject.transform;

            Vector3 pos = balltrans.position;

            pos.x=-0.8f*(3-i)+0.8f;

            balltrans.position = pos;
        }

        // キー入力があった時配列をソートする
        if(Input.anyKey){
            //myball.Sort ((a, b) => a.ballnumber - b.ballnumber);
            Array.Sort (myball, (a, b) => a.ballnumber - b.ballnumber);
        }
    }
}
