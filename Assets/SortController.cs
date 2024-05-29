using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using Unity.VisualScripting;

public class SortController : MonoBehaviour
{
    BallClass[] myBall = new BallClass[5];
    GameObject bar1;
    GameObject bar2;
    GameObject arrow;
    
    float timeInterval = 0.5f;
    int current_idx = 0;
    const float BallInterval = 0.8f;
    bool isSorging = false; //ソート実行中かを表すフラグ

    //https://t-stove-k.hatenablog.com/entry/2018/08/27/153609
    float GetAngle(Vector2 start,Vector2 target)
	{
		Vector2 dt = target - start;
		float rad = Mathf.Atan2 (dt.y, dt.x);
		float degree = rad * Mathf.Rad2Deg;
		
		return degree;
	}

    void Swap(BallClass[] balls, int idx1, int idx2)
    {
        // キューボールのオブジェクトを取得
        GameObject whiteBall = GameObject.Find("WhiteBall");
        BallClass ballA = balls[Math.Min(idx1, idx2)];
        BallClass ballB = balls[Math.Max(idx1, idx2)];

        Vector2 start = whiteBall.transform.position;
        Vector2 target = ballB.ballobject.transform.position;
        
        float BallAngle = GetAngle(start, target);

        WhiteBall.angle = BallAngle;
        WhiteBall.isMoving = true;
        WhiteBall.leftBall = ballA;
        WhiteBall.rightBall = ballB;

        // 実際のスワップ
        (balls[idx1], balls[idx2]) = (balls[idx2], balls[idx1]);
    }

    // 各ボールの下へバーを移動させる関数
    void MoveBar(BallClass b1, BallClass b2)
    {
        Vector3 bar1Pos = bar1.transform.position;
        Vector3 bar2Pos = bar2.transform.position;
        bar1Pos.x = b1.ballobject.transform.position.x;
        bar2Pos.x = b2.ballobject.transform.position.x;
        bar1Pos.y = b1.ballobject.transform.position.y-0.5f;
        bar2Pos.y = b2.ballobject.transform.position.y-0.5f;
        bar1.transform.position = bar1Pos;
        bar2.transform.position = bar2Pos;
    }

    IEnumerator BubbleSort()
    {
        isSorging = true;
        bar1.SetActive(true);
        bar2.SetActive(true);
        int myBallLen = myBall.Length;
        for (int j = myBallLen - 1; j >= current_idx + 1; j--)
        {
            BallClass ball1 = myBall[j - 1];
            BallClass ball2 = myBall[j];
            MoveBar(ball1, ball2); // バーをball1、ball2の下へ移動
            yield return new WaitForSeconds(timeInterval); //ボールを交換する前に遅延を入れる
            if (ball1.ballnumber > ball2.ballnumber)
            {
                Swap(myBall, j - 1, j);
                yield return new WaitForSeconds(0.4f / WhiteBall.speed);
            }
            yield return new WaitForSeconds(timeInterval); //ボールを交換した後に遅延を入れる
        }

        current_idx++;
        bar1.SetActive(false);
        bar2.SetActive(false);
        isSorging = false;
    }

    IEnumerator SelectionSort()
    {
        isSorging = true;
        bar1.SetActive(true);
        bar2.SetActive(true);
        int myBallLen = myBall.Length;
        int minIdx = current_idx;
        for (int j = current_idx + 1; j < myBallLen; j++)
        {
            BallClass ball1 = myBall[minIdx];
            BallClass ball2 = myBall[j];
            MoveBar(ball1, ball2);
            yield return new WaitForSeconds(timeInterval); // 遅延を入れる
            if (ball1.ballnumber > ball2.ballnumber) minIdx = j;
        }

        Swap(myBall, minIdx, current_idx);
        yield return new WaitForSeconds(0.4f / WhiteBall.speed); 
        current_idx++;
        bar1.SetActive(false);
        bar2.SetActive(false);
        isSorging = false;
    }


    IEnumerator InsertionSort()
    {
        isSorging = true;
<<<<<<< Updated upstream
        bar1.SetActive(true);
        bar2.SetActive(true);
        BallClass work = myBall[current_idx + 1];
=======
        BallClass work = myBall[current_idx + 1];//2つ目を上に飛ばす
>>>>>>> Stashed changes

        Vector3 workPos = work.ballobject.transform.position;

        Vector3 tmp = workPos;
        workPos.y = 3.5f;
        work.ballobject.transform.position = workPos;

        int j = current_idx;
<<<<<<< Updated upstream
        while (j >= 0 && myBall[j].ballnumber > work.ballnumber)
        {
            MoveBar(work, myBall[j]);
            yield return new WaitForSeconds(timeInterval); // 遅延を入れる
            myBall[j + 1] = myBall[j];
            j--;
        }
        
        yield return new WaitForSeconds(timeInterval); // 遅延を入れる
        work.ballobject.transform.position = tmp;
        myBall[j + 1] = work;
=======
        //while (j >= 0 && myBall[j].ballnumber > work.ballnumber)
        yield return new WaitForSeconds(timeInterval);
        while(j >= 0)
        {//1つ目から順に比較
            yield return new WaitForSeconds(timeInterval);
            if(WhiteBall.swapstart == false){
                bar1.SetActive(true);
                bar2.SetActive(true);
                MoveBar(work, myBall[j]);
                yield return new WaitForSeconds(timeInterval); // 遅延を入れる
                bar1.SetActive(false);
                bar2.SetActive(false);
                if(myBall[j]!=work){
                    if(myBall[j].ballnumber > work.ballnumber){
                        myBall[j + 1] = myBall[j];//配列側で入れかえ
                        WhiteBall.leftBall = myBall[j];//アニメーション対象のボールを定義
                        WhiteBall.swapstart = true;//アニメーション開始
                        yield return new WaitForSeconds(timeInterval);
                    }else{
                        break;
                    }
                }
                j--;
            }
        }
        WhiteBall.stat = 4;//workballを空いている場所に入れるアニメーション
        myBall[j+1] = work;//配列側でも空いている場所に入れる操作
            
        yield return new WaitForSeconds(timeInterval); // 遅延を入れる
>>>>>>> Stashed changes

        current_idx++;
        bar1.SetActive(false);
        bar2.SetActive(false);
        isSorging = false;
    }

    void Start()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("ball");

        // ボール番号を名前から整数で取得
        // BallClassとして取得
        for (int i = 0; i < 5; i++)
        {
            int ballNum = int.Parse(objects[i].name.Substring(5));
            myBall[i] = new BallClass(objects[i], ballNum);
        }

        // スワップするボールを示すバーと矢印を取得
        bar1 = GameObject.FindGameObjectsWithTag("bar")[0];
        bar2 = GameObject.FindGameObjectsWithTag("bar")[1];
        arrow = GameObject.FindGameObjectsWithTag("arrow")[0];
        bar1.SetActive(false);
        bar2.SetActive(false);
        arrow.SetActive(false);
    }

    void SyncBallPos()
    {
       // ボールの動きをBallClassの情報と連動させる
        for (int i = 0; i < 5; i++)
        {
            Transform ballTrans = myBall[i].ballobject.transform;
            Vector3 pos = ballTrans.position;
            pos.x = -BallInterval * (3 - i) + BallInterval;
            ballTrans.position = pos;
        } 
    }

    void Update()
    {
        if(!WhiteBall.isMoving)SyncBallPos();

        if (Input.GetKeyUp(KeyCode.Space))
        {
            int myBallLen = myBall.Length;
            if (current_idx >= myBallLen - 1)
            {
                // ソート完了時の処理
                Debug.Log("ソート完了");
            }
            else
            {
                // なんらかのソートを実行中でないならソートを始める
                if (!isSorging)
                {
                    if (SelectScr.SortKind == 1) StartCoroutine(BubbleSort());
                    else if (SelectScr.SortKind == 2) StartCoroutine(SelectionSort());
                    else if (SelectScr.SortKind == 3) StartCoroutine(InsertionSort());
                }
            }
        }
    }
}