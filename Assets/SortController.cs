using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SortController : MonoBehaviour
{
    BallClass[] MyBall = new BallClass[5];
    GameObject bar;
    GameObject arrow;

    int checkpos = 0;

    // バブルソートの手順を一つ進める関数
    void BubbleSorting()
    {
        var ballA = MyBall[checkpos];
        var ballB = MyBall[checkpos + 1];
        var ax = ballA.ballobject.transform.position.x;
        var bx = ballB.ballobject.transform.position.x;

        arrow.SetActive(false);
        if (MyBall[checkpos].ballnumber > MyBall[checkpos + 1].ballnumber)
        {
            // ボールをスワップ
            MyBall[checkpos] = ballB;
            MyBall[checkpos + 1] = ballA;

            // 矢印の位置を指定
            arrow.SetActive(true);
            Vector3 arrowpos = arrow.transform.position;
            arrowpos.x = (ax + bx) / 2;
            arrow.transform.position = arrowpos;
        }

        // バーの位置を設定
        bar.SetActive(true);
        Vector3 barpos = bar.transform.position;
        barpos.x = (ax + bx) / 2;
        bar.transform.position = barpos;

        checkpos = (checkpos + 1) % (MyBall.Length - 1);
    }

    void SelectSorting()
    {
        var ballA = MyBall[checkpos];
        var minBall = MyBall[checkpos];
        var ax = ballA.ballobject.transform.position.x;
        var bx = minBall.ballobject.transform.position.x;

        arrow.SetActive(false);
        if (MyBall[checkpos].ballnumber > MyBall[checkpos + 1].ballnumber)
        {
            // ボールをスワップ
            // MyBall[checkpos] = ballB;
            // MyBall[checkpos + 1] = ballA;

            // 矢印の位置を指定
            arrow.SetActive(true);
            Vector3 arrowpos = arrow.transform.position;
            arrowpos.x = (ax + bx) / 2;
            arrow.transform.position = arrowpos;
        }

        // バーの位置を設定
        bar.SetActive(true);
        Vector3 barPos = bar.transform.position;
        barPos.x = (ax + bx) / 2;
        bar.transform.position = barPos;

        checkpos = (checkpos + 1) % (MyBall.Length - 1);
    }

    void Start()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("ball");

        // ボール番号を名前から整数で取得
        // BallClassとして取得
        for (int i = 0; i < 5; i++)
        {
            int ballnum = int.Parse(objects[i].name.Substring(5));
            MyBall[i] = new BallClass(objects[i], ballnum);
        }

        // スワップされたボールを示すバーと矢印を取得
        bar = GameObject.FindGameObjectsWithTag("bar")[0];
        arrow = GameObject.FindGameObjectsWithTag("arrow")[0];
        bar.SetActive(false);
        arrow.SetActive(false);
    }

    float ballinterval = 0.8f;

    void Update()
    {
        // ボールの動きをBallClassの情報と連動させる
        for (int i = 0; i < 5; i++)
        {
            Transform balltrans = MyBall[i].ballobject.transform;

            Vector3 pos = balltrans.position;

            pos.x = -ballinterval * (3 - i) + ballinterval;

            balltrans.position = pos;
        }

        if (SelectScr.SortKind == 1)
        {
            // キー入力があった時配列をソートする
            if (Input.anyKeyDown)
            {
                BubbleSorting();
            }
        }
        else if (SelectScr.SortKind == 2)
        {
            // Select
        }
        else if (SelectScr.SortKind == 3)
        {
            // Insert
        }
    }
}