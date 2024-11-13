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

    public int[] myItem;//自分の使用するアイテム
    public int[] otherItem;//相手の使用するアイテム


    void Start()
    {
        networkSystem = FindObjectOfType<NetworkSystem>();
    }

    public void StartItemUsePhase()
    {
        //アイテム処理
        /*foreach(item in myitem){
            Debug.Log($"アイテム{item+1}を使用しました")
            ApplyItemEffect(item);
        }*/

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
