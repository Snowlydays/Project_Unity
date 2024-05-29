using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WhiteBall : MonoBehaviour
{
    public static bool isMoving = false;
    public static float angle;

    public static bool swapstart = false;
    public static BallClass leftBall;
    public static BallClass rightBall;
    Vector2 leftBallPos;
    Vector2 rightBallPos;
    public static BallClass workBall;
    Vector2 workBallPos;
    Vector2 midPos;
    public static float speed = 0.2f;
    bool gettingPos = false; // 一回実行のためのフラグ
    public static int stat = 1;
    float rotationAngle = 0; // ボールの回転位置
    Vector3 TempBallPos;

    float GetAngle(Vector2 start,Vector2 target)
	{
		Vector2 dt = target - start;
		float rad = Mathf.Atan2 (dt.y, dt.x);
		float degree = rad * Mathf.Rad2Deg;
		
		return degree;
	}

    Vector2 resultPosition()
    {
        float r = rightBallPos.x - midPos.x;
        float sin = Mathf.Sin(Mathf.Deg2Rad * rotationAngle);
        float cos = Mathf.Cos(Mathf.Deg2Rad * rotationAngle);
        Vector2 resPos = new Vector2(
            midPos.x + r * cos,
            midPos.y + r * sin
        );
        return resPos;
    }
    void FixedUpdate()
    {
        if(isMoving){
            if(SelectScr.SortKind != 3){
                if(!gettingPos)
                {
                    leftBallPos = leftBall.ballobject.transform.position;
                    rightBallPos = rightBall.ballobject.transform.position;
                    midPos = (leftBallPos + rightBallPos) / 2;
                    gettingPos = true;
                }           
                if(stat == 1) // キューボールが最初に飛んでいく
                {
                    Vector3 addTrans = new Vector3(
                        speed*Mathf.Cos(angle * Mathf.Deg2Rad),
                        speed*Mathf.Sin(angle * Mathf.Deg2Rad),
                        0
                    );
                    this.gameObject.transform.position += addTrans;

                    if(Math.Abs(this.gameObject.transform.position.y - rightBall.ballobject.transform.position.y) < 0.1)
                    {
                        stat = 2;
                        this.gameObject.transform.position = rightBallPos; // 位置を固定
                    }
                }
                else if(stat == 2) // 上半分の回転
                {
                    rotationAngle += speed * 60;
                    rightBall.ballobject.transform.position = resultPosition();

                    if(Math.Abs(rightBall.ballobject.transform.position.x - leftBallPos.x) < 0.1f
                    && Math.Abs(rightBall.ballobject.transform.position.y - leftBallPos.y) < 0.1f)
                    {
                        stat = 3;
                        rightBall.ballobject.transform.position = leftBallPos; // 位置を固定
                    }
                }
                else if(stat == 3) // 下半分の回転
                {
                    rotationAngle += speed * 60;
                    leftBall.ballobject.transform.position = resultPosition();

                    if(Math.Abs(leftBall.ballobject.transform.position.x - rightBallPos.x) < 0.1f
                    && Math.Abs(leftBall.ballobject.transform.position.y - rightBallPos.y) < 0.1f)
                    {
                        stat = 4;
                        leftBall.ballobject.transform.position = rightBallPos; // 位置を固定
                    }
                }
                else if(stat == 4) // キューボールが画面外に飛んでいく
                {
                    Vector3 addTrans = new Vector3(
                        speed*Mathf.Cos(angle * Mathf.Deg2Rad),
                        speed*Mathf.Sin(angle * Mathf.Deg2Rad),
                        0
                    );

                    this.gameObject.transform.position += addTrans;
                    if(this.gameObject.transform.position.y > 6.0f)
                    {
                        stat = 5;

                        Vector3 outOfScreen = new Vector3(0f, -6.0f, 0);
                        this.gameObject.transform.position = outOfScreen;
                    }
                }
                else if(stat == 5) // キューボールが最初の位置に戻る
                {
                    Vector3 addTrans = new Vector3(0, speed, 0);
                    this.gameObject.transform.position += addTrans;

                    if(this.gameObject.transform.position.y > -0.8f)
                    {
                        stat = 1;
                        isMoving = false;
                        gettingPos = false;
                        
                        Vector3 defaultPosition = new Vector3(0f, -0.8f, 0);
                        this.gameObject.transform.position = defaultPosition;
                    }
                }
            }else{
                //InsertAnimation
                if(!gettingPos)
                {
                    workBallPos = workBall.ballobject.transform.position;
                    gettingPos = true;
                }   
                if(stat == 1) // キューボールが最初に飛んでいく
                {
                    Vector3 addTrans = new Vector3(
                        speed*Mathf.Cos(angle * Mathf.Deg2Rad),
                        speed*Mathf.Sin(angle * Mathf.Deg2Rad),
                        0
                    );
                    this.gameObject.transform.position += addTrans;

                    if(Math.Abs(this.gameObject.transform.position.y - workBall.ballobject.transform.position.y) < 0.1)
                    {
                        stat = 2;
                        this.gameObject.transform.position = workBallPos; // 位置を固定
                    }
                }else if(stat == 2) 
                {
                    Vector3 workBallAfterPos = workBallPos;
                    TempBallPos = workBallPos;
                    workBallAfterPos.y += 2.0f;

                    Vector3 addTrans = new Vector3(
                        0,
                        speed,
                        0
                    );

                    workBall.ballobject.transform.position += addTrans;
                    if(workBall.ballobject.transform.position.y > workBallAfterPos.y)
                    {
                        stat = 3;
                        workBall.ballobject.transform.position = workBallAfterPos;
                    }
                }else if(stat == 3){
                    Vector3 outOfScreen = new Vector3(0f, -6.0f, 0);
                    this.gameObject.transform.position = outOfScreen;

                    if(swapstart==true){
                        Vector3 addTrans = new Vector3(speed,0,0);
                        leftBall.ballobject.transform.position += addTrans;

                        if(leftBall.ballobject.transform.position.x > TempBallPos.x)
                        {
                            swapstart=false;
                            leftBall.ballobject.transform.position = TempBallPos;
                            TempBallPos.x -= 0.8f;
                        }
                    }
                }else if(stat == 4){
                    angle = GetAngle(workBall.ballobject.transform.position,TempBallPos);
                    Vector3 addTrans = new Vector3(
                        speed*Mathf.Cos(angle * Mathf.Deg2Rad),
                        speed*Mathf.Sin(angle * Mathf.Deg2Rad),
                        0
                    );

                    workBall.ballobject.transform.position += addTrans;
                    if(Math.Abs(TempBallPos.y - workBall.ballobject.transform.position.y) < 0.2)
                    {
                        stat = 5;
                        workBall.ballobject.transform.position = TempBallPos;
                    }
                }else if(stat == 5){
                    Vector3 addTrans = new Vector3(0, speed, 0);
                    this.gameObject.transform.position += addTrans;

                    if(this.gameObject.transform.position.y > -0.8f)
                    {
                        stat = 1;
                        isMoving = false;
                        gettingPos = false;
                        
                        Vector3 defaultPosition = new Vector3(0f, -0.8f, 0);
                        this.gameObject.transform.position = defaultPosition;
                    }
                }
            }
        }
    }
}
