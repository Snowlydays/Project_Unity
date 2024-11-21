using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    //何らかのアニメーションをするオブジェクト全てを総括的に処理するスクリプト

    public void OnPhaseAnimationEnd()
    {
        //フェーズ開始時のロゴアニメーション終了時の処理
        //自身を削除してシーン内にオブジェクトが溜まるのを防止
        Destroy(this.gameObject);
    }
}
