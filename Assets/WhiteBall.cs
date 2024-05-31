using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class WhiteBall : MonoBehaviour
{
    public static bool isMoving = false;
    public static float angle;

    public static bool swapstart = false;
    public static BallClass leftBall;
    public static BallClass rightBall;
    public static BallClass workBall;
    Vector2 leftBallPos;
    Vector2 rightBallPos;
    Vector2 workBallPos;
    Vector2 midPos;
    Vector3 TempBallPos;
    public static float speed = 0.15f; //回転する速度
    bool gettingPos = false; // 一回実行のためのフラグ
    public static int stat = 1;

    float topRotationAngle = 0; // ボールの回転位置
    float bottomRotationAngle = 180; // ボールの回転位置
    bool isMovedRightBall = false, isMovedLeftBall = false, areSwapBallsTouching = false, isWhiteBallTouching = false;


    float GetAngle(Vector2 start, Vector2 target)
    {
        Vector2 dt = target - start;
        float rad = Mathf.Atan2(dt.y, dt.x);
        float degree = rad * Mathf.Rad2Deg;

        return degree;
    }

    Vector2 resultPosition(float rotationAngle)
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

    bool BallsTouching(Vector2 pos1, Vector2 pos2)
    {
        float r = this.gameObject.transform.localScale.x; // キューボールの直径取得(他のボールも同じサイズなのでこの値を使用する)
        return Vector2.Distance(pos1, pos2) <= r;
    }

    // targetBallへキューボールが飛んでいく関数
    void MoveCueBall(BallClass targetBall)
    {
        Vector3 addTrans = new Vector3(speed * Mathf.Cos(angle * Mathf.Deg2Rad),
            speed * Mathf.Sin(angle * Mathf.Deg2Rad), 0);
        this.gameObject.transform.position += addTrans;

        if (BallsTouching(this.gameObject.transform.position, targetBall.ballobject.transform.position))
        {
            stat = 2;
            transform.position = Vector3.MoveTowards(transform.position, targetBall.ballobject.transform.position,
                speed * Time.deltaTime);
            isWhiteBallTouching = true;
        }
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            // キューボールが画面外に飛んでいく
            if (isWhiteBallTouching)
            {
                // 直進ver
                Vector3 addTrans = new Vector3(
                    speed * Mathf.Cos(angle * Mathf.Deg2Rad),
                    speed * Mathf.Sin(angle * Mathf.Deg2Rad),
                    0
                );

                this.gameObject.transform.position += addTrans;
                if (this.gameObject.transform.position.y > 6.0f)
                {
                    Vector3 outOfScreen = new Vector3(0f, -6.0f, 0);
                    this.gameObject.transform.position = outOfScreen;
                    isWhiteBallTouching = false;
                }

                // 反射ver
                // Vector3 addTrans = new Vector3(speed * Mathf.Cos(angle * Mathf.Deg2Rad),
                //     -speed * Mathf.Sin(angle * Mathf.Deg2Rad), 0);
                // this.gameObject.transform.position += addTrans;
                // Vector2 whiteBallPos = this.gameObject.transform.position;
                // if (whiteBallPos.x > 6.0f || whiteBallPos.x < -6.0f || whiteBallPos.y > 6.0f || whiteBallPos.y < -6.0f)
                // {
                //     Vector3 outOfScreen = new Vector3(0f, -6.0f, 0);
                //     this.gameObject.transform.position = outOfScreen;
                //     isWhiteBallTouching = false;
                // }
            }

            if (SelectScr.SortKind != 3)
            {
                if (!gettingPos)
                {
                    leftBallPos = leftBall.ballobject.transform.position;
                    rightBallPos = rightBall.ballobject.transform.position;
                    midPos = (leftBallPos + rightBallPos) / 2;
                    gettingPos = true;
                }

                if (stat == 1)
                {
                    MoveCueBall(rightBall);
                }
                else if (stat == 2) // 回転
                {
                    if (!isMovedRightBall)
                    {
                        topRotationAngle += speed * 20;
                        rightBall.ballobject.transform.position = resultPosition(topRotationAngle);
                        if (Math.Abs(rightBall.ballobject.transform.position.x - leftBallPos.x) < 0.05f
                            && Math.Abs(rightBall.ballobject.transform.position.y - leftBallPos.y) < 0.05f)
                        {
                            isMovedRightBall = true;
                            rightBall.ballobject.transform.position = leftBallPos; // 位置を固定
                        }
                    }

                    if (BallsTouching(rightBall.ballobject.transform.position, leftBallPos))
                        areSwapBallsTouching = true;

                    if (areSwapBallsTouching && !isMovedLeftBall)
                    {
                        bottomRotationAngle += speed * 20;
                        leftBall.ballobject.transform.position = resultPosition(bottomRotationAngle);
                        if (Math.Abs(leftBall.ballobject.transform.position.x - rightBallPos.x) < 0.05f
                            && Math.Abs(leftBall.ballobject.transform.position.y - rightBallPos.y) < 0.05f)
                        {
                            isMovedLeftBall = true;
                            leftBall.ballobject.transform.position = rightBallPos; // 位置を固定
                        }
                    }

                    if (isMovedRightBall && isMovedLeftBall) stat = 5;
                }
                else if (stat == 3)
                {
                }
                else if (stat == 4)
                {
                }
                else if (stat == 5) // キューボールが最初の位置に戻る
                {
                    Vector3 addTrans = new Vector3(0, speed, 0);
                    this.gameObject.transform.position += addTrans;

                    if (this.gameObject.transform.position.y > -0.8f)
                    {
                        stat = 1;
                        isMoving = false;
                        gettingPos = false;
                        isMovedRightBall = isMovedLeftBall = areSwapBallsTouching = false; //初期化
                        (topRotationAngle, bottomRotationAngle) = (0, 180); //初期化
                        Vector3 defaultPosition = new Vector3(0f, -0.8f, 0);
                        this.gameObject.transform.position = defaultPosition;
                    }
                }
            }
            else
            {
                //InsertAnimation
                if (!gettingPos)
                {
                    workBallPos = workBall.ballobject.transform.position;
                    gettingPos = true;
                }

                if (stat == 1) // キューボールが最初に飛んでいく
                {
                    MoveCueBall(workBall);
                }
                else if (stat == 2)
                {
                    Vector3 workBallAfterPos = workBallPos;
                    TempBallPos = workBallPos;
                    workBallAfterPos.y += 2.0f;

                    Vector3 addTrans = new Vector3(0, speed, 0);

                    workBall.ballobject.transform.position += addTrans;
                    if (workBall.ballobject.transform.position.y > workBallAfterPos.y)
                    {
                        stat = 3;
                        workBall.ballobject.transform.position = workBallAfterPos;
                    }
                }
                else if (stat == 3)
                {
                    Vector3 outOfScreen = new Vector3(0f, -6.0f, 0);
                    this.gameObject.transform.position = outOfScreen;

                    if (swapstart == true)
                    {
                        Vector3 addTrans = new Vector3(speed, 0, 0);
                        leftBall.ballobject.transform.position += addTrans;

                        if (leftBall.ballobject.transform.position.x > TempBallPos.x)
                        {
                            swapstart = false;
                            leftBall.ballobject.transform.position = TempBallPos;
                            TempBallPos.x -= 0.8f;
                        }
                    }
                }
                else if (stat == 4)
                {
                    angle = GetAngle(workBall.ballobject.transform.position, TempBallPos);
                    Vector3 addTrans = new Vector3(
                        speed * Mathf.Cos(angle * Mathf.Deg2Rad),
                        speed * Mathf.Sin(angle * Mathf.Deg2Rad),
                        0
                    );

                    workBall.ballobject.transform.position += addTrans;
                    if (Math.Abs(TempBallPos.y - workBall.ballobject.transform.position.y) < 0.2)
                    {
                        stat = 5;
                        workBall.ballobject.transform.position = TempBallPos;
                    }
                }
                else if (stat == 5)
                {
                    Vector3 addTrans = new Vector3(0, speed, 0);
                    this.gameObject.transform.position += addTrans;

                    if (this.gameObject.transform.position.y > -0.8f)
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