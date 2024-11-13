using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemUsingManager : MonoBehaviour
{
    //アイテム効果を処理するスクリプト
    //ItemPhaseManagerでどのアイテムをどんな順序で使用するかを決めた後、
    //その情報をここに送る流れにした
    //3の強奪系、1,5の即時発動系などをItemUsingPhase内で順序立てて処理できるように設計

    private NetworkSystem networkSystem;

    public int[] myItems;//自分の使用するアイテム
    public int[] otherItems;//相手の使用するアイテム

    List<int> mylist;
    List<int> otherlist;


    void Start()
    {
        networkSystem = FindObjectOfType<NetworkSystem>();
    }

    public void StartItemUsePhase()
    {
        if(myItems.Length<=0){
            //自分がアイテムを選択していなかったら強制終了
            Debug.Log("アイテムを選択していなかったので終了します");
            //networkSystem.ToggleReady();
            //return;
        }
        
        mylist = new List<int>(myItems);
        otherlist = new List<int>(otherItems);

        Debug.Log("自分のアイテム");
        foreach(int item in mylist)
        {
            Debug.Log(item);
        }
        Debug.Log("相手のアイテム");
        foreach(int item in otherlist)
        {
            Debug.Log(item);
        }
        //アイテム処理
        /*foreach(item in myitem){
            Debug.Log($"アイテム{item+1}を使用しました")
            ApplyItemEffect(item);
        }*/


        //networkSystem.ToggleReady();
    }

    private void ApplyItemEffect(int itemIdx)
    {
        // ここにアイテムの効果を実装
        switch(itemIdx)
        {
            case 0:

            break;

            case 1:
                networkSystem.qutstionController.isGetDiff=true;
            break;

            case 2:

            break;

            case 3:

            break;

            case 4:

            break;

            case 5:
                networkSystem.qutstionController.isThreeSelect=true;
            break;
        }
    }
}
