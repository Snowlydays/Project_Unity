using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BubbleSort : MonoBehaviour
{
    BallClass[] myball = new BallClass[5];
    GameObject bar;
    GameObject arrow;

    int checkpos=0;
    // バブルソートの手順を一つ進める関数
    void AdvanceSorting()
    {
        var ballA=myball[checkpos];
        var ballB=myball[checkpos+1];
        var ax=ballA.ballobject.transform.position.x;
        var bx=ballB.ballobject.transform.position.x;
        
        arrow.SetActive(false);
        if(myball[checkpos].ballnumber>myball[checkpos+1].ballnumber)
        {
            // ボールをスワップ
            myball[checkpos]=ballB;
            myball[checkpos+1]=ballA;
            Debug.Log("swapped");
            
            // 矢印の位置を指定
            arrow.SetActive(true);
            Vector3 arrowpos=arrow.transform.position;
            arrowpos.x=(ax+bx)/2;
            arrow.transform.position=arrowpos;
        }

        // バーの位置を設定
        bar.SetActive(true);
        Vector3 barpos=bar.transform.position;
        barpos.x=(ax+bx)/2;
        bar.transform.position=barpos;

        checkpos=(checkpos+1)%(myball.Length-1);
    }

    void Start()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("ball");

        // ボール番号を名前から整数で取得
        // BallClassとして取得
        for(int i=0;i<5;i++){
            int ballnum = int.Parse(objects[i].name.Substring(5));
            myball[i] = new BallClass(objects[i],ballnum);
        }

        // スワップされたボールを示すバーと矢印を取得
        bar = GameObject.FindGameObjectsWithTag("bar")[0];
        arrow = GameObject.FindGameObjectsWithTag("arrow")[0];
        bar.SetActive(false);
        arrow.SetActive(false);
    }

    float ballinterval=0.8f;
    bool swapped=false;
    void Update()
    {
        // ボールの動きをBallClassの情報と連動させる
        for(int i=0;i<5;i++){
            Transform balltrans = myball[i].ballobject.transform;

            Vector3 pos = balltrans.position;

            pos.x=-ballinterval*(3-i)+ballinterval;

            balltrans.position = pos;
        }

        // キー入力があった時配列をソートする
        if(Input.anyKey){
            //myball.Sort ((a, b) => a.ballnumber - b.ballnumber);
            //Array.Sort (myball, (a, b) => a.ballnumber - b.ballnumber);
            if(!swapped)AdvanceSorting();
            swapped=true;
        }
        else swapped=false;
    }
}
