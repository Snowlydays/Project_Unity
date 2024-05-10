using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ballclass{

    //オブジェクトそのものとそのオブジェクトの番号をクラスでまとめて管理して
    //配列管理することで上手くいくのではないかと予想

    public GameObject ballobject;
    public int ballnumber;
    public ballclass(GameObject objball,int num){
        ballobject = objball;
        ballnumber = num;
    }
}