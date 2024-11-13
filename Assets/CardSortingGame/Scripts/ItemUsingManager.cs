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
            networkSystem.ToggleReady();
            return;
        }
        
        mylist = new List<int>(myItems);
        otherlist = new List<int>(otherItems);

        Debug.Log("自分のアイテム");
        foreach(int item in mylist)
        {
            Debug.Log($"アイテム{item+1}");
        }
        Debug.Log("相手のアイテム");
        if(otherlist.Count>0){
            foreach(int item in otherlist)
            {
                Debug.Log($"アイテム{item+1}");
            }
        }else{
            Debug.Log("相手はアイテムを選択していません");
        }

        ItemThreeCheckAndUse();//アイテム3効果を処理する部分

        Debug.Log("アイテム3処理後");
        Debug.Log("自分のアイテム");
        foreach(int item in mylist)
        {
            Debug.Log($"アイテム{item+1}");
        }
        Debug.Log("相手のアイテム");
        if(otherlist.Count>0){
            foreach(int item in otherlist)
            {
                Debug.Log($"アイテム{item+1}");
            }
        }else{
            Debug.Log("相手はアイテムを選択していません");
        }

        foreach(int item in mylist){
            Debug.Log($"アイテム{item+1}を使用しました");
            ApplyItemEffect(item);
        }

        networkSystem.ToggleReady();//ここでreadyをtrueにしているはずなのにtrueにならないことがある(なので質問フェーズに移行せずフリーズする)
        //原因わかる人がいたら教えて欲しい
    }

    private void ItemThreeCheckAndUse(){
        int mynum=mylist.Count;//自分の使用するアイテム数を取得
        int othernum=otherlist.Count;//相手の使用するアイテム数を取得
        int mythreeindex=mylist.IndexOf(2);//自分が使用するアイテム3のindexを取得(1番目なら0、2番目なら1...)
        int otherthreeindex=otherlist.IndexOf(2);//相手も同様
        int mygetitem = -1;//自分が奪ったアイテム
        int othergetitem = -1;//相手が奪ったアイテム

        bool myusethree=false,otherusethree=false;//アイテム3処理をするかどうか

        if(mythreeindex!=-1)myusethree=true;//アイテム3が見つかったかつ相手が何かしらアイテムを使っていたら使用bool値をtrueにする
        if(otherthreeindex!=-1)otherusethree=true;//相手のも同様

        if(othernum<=0 && myusethree==true){
            //自分がアイテム3を使おうとしているのに相手がそもそもアイテムを使わない場合
            //リストからアイテム3をremoveして終了
            myusethree=false;
            mylist.Remove(2);
            return;
        }
        //自分が使わない場合はそもそもこのメソッドが実行されないので省略

        if(mythreeindex+1>othernum && myusethree==true){
            //アイテム3が相手のアイテム数より大きいindexを奪おうとした場合
            //アイテム3の奪うindex先を相手のアイテムの先頭にする
            mythreeindex=othernum-1;
            //もし、この状態で相手の先頭アイテムがアイテム3だった場合
            //アイテム3の位置を相手の先頭に移動させる
            if(otherlist[mythreeindex]==2)MyMoveToIndex(othernum-1);
        }

        if(otherthreeindex+1>mynum && otherusethree==true){
            //相手の場合も同様
            otherthreeindex=mynum-1;
            if(mylist[otherthreeindex]==2)OtherMoveToIndex(mynum-1);
        }

        if(myusethree==true)mygetitem = otherlist[mythreeindex];//指定場所のアイテムを取得
        if(otherusethree==true)othergetitem = mylist[otherthreeindex];

        //アイテム3を使用してない場合はそもそも処理しない
        if(mygetitem!=-1){
            //取得したアイテムが3でなければ、3があった場所に奪ったアイテムを入れ替える
            //その後奪った相手のアイテムをremove

            if(mygetitem!=2)mylist[mylist.IndexOf(2)]=mygetitem;
            otherlist.Remove(mygetitem);
        }

        if(othergetitem!=-1){
            //相手も同様
            if(othergetitem!=2)otherlist[otherlist.IndexOf(2)]=othergetitem;
            mylist.Remove(othergetitem);
        }
    }

    //アイテム3の位置を変更するのに使用するメソッド
    private void MyMoveToIndex(int index){
        int tempind=mylist.IndexOf(2);
        while(tempind!=index){
            int temp=mylist[tempind];
            mylist[tempind]=mylist[tempind-1];
            mylist[tempind-1]=temp;
            tempind--;
        }
        Debug.Log("並び替え完了");
    }

    private void OtherMoveToIndex(int index){
        int tempind=otherlist.IndexOf(2);
        while(tempind!=index){
            int temp=otherlist[tempind];
            otherlist[tempind]=otherlist[tempind-1];
            otherlist[tempind-1]=temp;
            tempind--;
        }
        Debug.Log("並び替え完了");
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
                //アイテム3の効果はItemThreeCheckAndUseにて行うのでここには何も入れない
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
