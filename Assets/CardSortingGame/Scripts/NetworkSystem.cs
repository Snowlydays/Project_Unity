using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSystem : MonoBehaviour//NetworkBehaviour
{
    //現在のフェーズが何であるかをintのnetworkvariableで管理する。
    //0 その他のフェーズ(基本的にはどんな操作も受け付けさせたくないような状態にしたいときに使うのを想定) 
    //例えば、切断時や、何らかの演出が発生している場面など、その最中に何らかの操作をされると困るような場面全般
    //1 アイテムフェーズ  2 質問・詠唱フェーズ
    //private NetworkVariable<int> netphase;

    public static int phase = 0;//現在フェーズを確認したい時はこれを参照する。読み取り専用
    //networkvariableで管理するが、参照する際は上のphase変数を仲介する形で行うと上手くいくと予想。
    //現時点ではnetworkvariableは使用せず、あくまで仮置き

    /*void Awake()
    {
       netphase = new NetworkVariable<int>(0);
    }

    public override void OnNetworkSpawn()
    {
        // NetworkListの初期化と変更イベントの登録
        netphase.OnValueChanged += (int oldParam, int newParam) =>
        {
            phase = newParam;
        };
    }*/

    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha0)){
            phase=0;
            Debug.Log("changed phase 0");
        }else if(Input.GetKeyDown(KeyCode.Alpha1)){
            phase=1;
            Debug.Log("changed phase 1");
        }else if(Input.GetKeyDown(KeyCode.Alpha2)){
            phase=2;
            Debug.Log("changed phase 2");
        }
    }
    
}
