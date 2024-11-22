using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class ItemUsingManager : MonoBehaviour
{
    //アイテム効果を処理するスクリプト
    //ItemPhaseManagerでどのアイテムをどんな順序で使用するかを決めた後、
    //その情報をここに送る流れにした
    //3の強奪系、1,5の即時発動系などをItemUsingPhase内で順序立てて処理できるように設計

    private NetworkSystem networkSystem;
    private CardsManager cardsManager;
    private MainSystemScript mainSystemScript;

    public int[] myItems;//自分の使用するアイテム
    public int[] otherItems;//相手の使用するアイテム

    List<int> mylist;
    List<int> otherlist;

    GameObject ItemSixBG;
    GameObject ItemOneBG;
    GameObject selectCard;
    private Transform cardPanel;

    GameObject smallerButton;
    GameObject largerButton;

    GameObject[] clonedCards;

    public int nowUsingItem=-1;//どのアイテムを使っているか(-1で何も使っていない状態)

    bool confirmchecked=false;//confirmされたかどうか

    bool allUsedItem=false;//アイテムを全て使い切ったかどうか

    TMP_InputField inputField;

    int chooseNumber=-1;

    int chooseCompareTo=0;//アイテム1で大小どちらへ比較するか、1で大、-1で小、0で無選択

    public GameObject itemUseCanvas;

    void Start()
    {
        cardsManager = FindObjectOfType<CardsManager>();
        networkSystem = FindObjectOfType<NetworkSystem>();

        //各種UI初期化
        ItemSixBG = GameObject.Find("ItemSixBG");
        ItemOneBG = GameObject.Find("ItemOneBG");
        cardPanel = GameObject.Find("ItemUsingCardPanel").transform;

        GameObject.Find("ConfirmButtonOne").GetComponent<Button>().onClick.AddListener(OnConfirmButtonClickedOne);
        GameObject.Find("ConfirmButtonSix").GetComponent<Button>().onClick.AddListener(OnConfirmButtonClickedSix);

        smallerButton=GameObject.Find("SmallerButton");
        smallerButton.GetComponent<Button>().onClick.AddListener(OnSmallerButtonClicked);
        largerButton=GameObject.Find("LargerButton");
        largerButton.GetComponent<Button>().onClick.AddListener(OnLargerButtonClicked);

        inputField = GameObject.Find("InputText").GetComponent<TMP_InputField>();

        itemUseCanvas = GameObject.Find("ItemCanvas");

        ItemSixBG.SetActive(false);
        ItemOneBG.SetActive(false);
        cardPanel.GameObject().SetActive(false);
    }

    public void InputNumber()
    {
        //入力されている文字が数値ならその数値を、ぞれ以外なら-1とする
        if (!int.TryParse(inputField.text,out chooseNumber)){
            chooseNumber=-1;
            inputField.text="";
        }
        if(chooseNumber<1 || chooseNumber>NetworkSystem.cardNum){
            chooseNumber=-1;
            inputField.text="";
        }
    }

    public void StartItemUsePhase(){
        StartCoroutine(StartItemUsePhaseIEnumerator());
    }

    IEnumerator StartItemUsePhaseIEnumerator()
    {
        mylist = new List<int>(myItems);
        otherlist = new List<int>(otherItems);

        /*Debug.Log("自分のアイテム");
        if(mylist.Count>0){
            foreach(int item in mylist)
            {
                Debug.Log($"アイテム{item+1}");
            }
        }else{
            Debug.Log("自分はアイテムを選択していません");
        }
        Debug.Log("相手のアイテム");
        if(otherlist.Count>0){
            foreach(int item in otherlist)
            {
                Debug.Log($"アイテム{item+1}");
            }
        }else{
            Debug.Log("相手はアイテムを選択していません");
        }*/

        ItemThreeCheckAndUse();//アイテム3効果を処理する部分

        /*Debug.Log("アイテム3処理後");
        Debug.Log("自分のアイテム");
        if(mylist.Count>0){
            foreach(int item in mylist)
            {
                Debug.Log($"アイテム{item+1}");
            }
        }else{
            Debug.Log("自分はアイテムを選択していません");
        }
        Debug.Log("相手のアイテム");
        if(otherlist.Count>0){
            foreach(int item in otherlist)
            {
                Debug.Log($"アイテム{item+1}");
            }
        }else{
            Debug.Log("相手はアイテムを選択していません");
        }*/

        ItemFourCheckAndUse();//相手がアイテム4を使っているかどうかと実行処理

        allUsedItem=false;

        StartCoroutine(ItemUseAwaiting());

        yield return new WaitUntil(() => allUsedItem==true);

        Debug.Log("アイテムを全て使い切りました");

        allUsedItem=false;
        networkSystem.ToggleReady();
    }

    private void ItemFourCheckAndUse(){
        if(otherlist.Contains(3)){
            //もし相手がアイテム4を持っていたら、質問不可能bool値をtrueにする。
            networkSystem.questionController.isNotQuestion=true;
            networkSystem.informationManager.AddInformationText("相手のアイテム効果により質問できません!");
        }
    }

    IEnumerator ItemUseAwaiting(){
        foreach(int item in mylist){
            nowUsingItem=item;
            Debug.Log($"アイテム{item+1}を使用しました");
            networkSystem.Log($"アイテム{item+1}を使用しました");
            ApplyItemEffect(item);
            //アイテム処理が終わったらnowUsingItem=-1;にして次のアイテム処理に移行させる。
            yield return new WaitUntil(() => nowUsingItem==-1);
        }
        allUsedItem=true;
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
        
        if(mynum<=0 && otherusethree==true){
            //相手がアイテム3を使おうとしているのに自分がそもそもアイテムを使わない場合
            //リストからアイテム3をremoveして終了
            otherusethree=false;
            otherlist.Remove(2);
            return;
        }

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
        //Debug.Log("並び替え完了");
    }

    private void OtherMoveToIndex(int index){
        int tempind=otherlist.IndexOf(2);
        while(tempind!=index){
            int temp=otherlist[tempind];
            otherlist[tempind]=otherlist[tempind-1];
            otherlist[tempind-1]=temp;
            tempind--;
        }
       //Debug.Log("並び替え完了");
    }

    private void ApplyItemEffect(int itemIdx)
    {
        // ここにアイテムの効果を実装
        switch(itemIdx)
        {
            case 0:
                StartCoroutine(ItemUseOne());
            break;

            case 1:
                networkSystem.questionController.isGetDiff=true;
                nowUsingItem=-1;
            break;

            //2,3は外部処理のためここには記述なし

            case 4:
                networkSystem.questionController.isThreeSelect=true;
                nowUsingItem=-1;
            break;

            case 5:
                StartCoroutine(ItemUseSix());
            break;

            default:
                nowUsingItem=-1;
            break;
        }
    }

    IEnumerator ItemUseOne()
    {
        //各種UIの起動
        ItemOneBG.SetActive(true);//起動
        cardPanel.GameObject().SetActive(true);
        clonedCards = cardsManager.PlaceCardsOnPanel(cardPanel,ToggleCardSelection);
        
        // clonedCards = cardsManager.CloneMyCardsAsUI();
        // if(clonedCards == null)Debug.LogError("clonedCards are null!");

        // キャンバスを探す
        // Canvas canvas = GameObject.Find("ItemCanvas").GetComponent<Canvas>();
            
        // foreach(GameObject card in clonedCards)
        // {
        //     // キャンバスにカードを追加
        //     card.transform.SetParent(canvas.transform);
        //
        //     // カードをボタンとして設定
        //     Button cardButton = card.GetComponent<Button>();
        //     cardButton.onClick.AddListener(() => ToggleCardSelection(card));
        // }

        Debug.Log("カードと大小を選択してください");

        while(!confirmchecked){
            yield return null;
        }

        //結果とUIの停止
        string cardname=selectCard.name;
        Debug.Log(cardname);
        
        int cardNum=int.Parse(cardname[cardname.Length-1].ToString());//カード番号を取得

        //そのカード番号が書かれているカードのindexを取得する。
        int startIndex=GetCardIndexInList(cardNum);

        Debug.Log(cardNum);

        if(startIndex==-1)Debug.LogError("カードが見つかりませんでした");

        if(chooseCompareTo==1){
            //大選択
            while(startIndex<NetworkSystem.cardNum-1){
                if(cardsManager.myCards[startIndex].cardNum>cardsManager.myCards[startIndex+1].cardNum)
                {
                    SwapCardAndList(startIndex,startIndex+1);
                    startIndex++;
                }else{
                    break;
                }
            }
        }
        if(chooseCompareTo==-1){
            //小選択
            while(startIndex>0){
                if(cardsManager.myCards[startIndex].cardNum<cardsManager.myCards[startIndex-1].cardNum)
                {
                    SwapCardAndList(startIndex,startIndex-1);
                    startIndex--;
                }else{
                    break;
                }
            }
        }

        foreach (GameObject card in clonedCards) Destroy(card);

        chooseCompareTo=0;
        selectCard=null;
        nowUsingItem=-1;
        confirmchecked=false;
        ItemOneBG.SetActive(false);
        cardPanel.GameObject().SetActive(false);
    }

    // 番号からカードのインデックスを取得
    private int GetCardIndexInList(int number)
    {
        for (int i = 0; i < cardsManager.myCards.Count; i++){
            if (cardsManager.myCards[i].cardNum == number){
                Debug.Log("見つけました");
                return i;
            }
        }
        return -1;
    }

    private void SwapCardAndList(int toIndex,int fromIndex)
    {
        Transform toSlot = cardsManager.myCards[toIndex].cardObject.transform.parent;
        Transform fromSlot = cardsManager.myCards[fromIndex].cardObject.transform.parent;

        //位置の並び替え
        cardsManager.myCards[toIndex].cardObject.transform.SetParent(fromSlot, false);
        cardsManager.myCards[toIndex].cardObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        cardsManager.myCards[toIndex].cardObject.transform.SetParent(toSlot, false);
        cardsManager.myCards[toIndex].cardObject.transform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        //配列の並び替え
        cardsManager.SwapCardsInList(toIndex, fromIndex);
    }

    IEnumerator ItemUseSix()
    {
        //各種UIの起動
        ItemSixBG.SetActive(true);//起動
        cardPanel.GameObject().SetActive(true);
        clonedCards = cardsManager.PlaceCardsOnPanel(cardPanel,ToggleCardSelection);
        // clonedCards = cardsManager.CloneMyCardsAsUI();
        // if(clonedCards == null)Debug.LogError("clonedCards are null!");

        // // キャンバスを探す
        // Canvas canvas = GameObject.Find("ItemCanvas").GetComponent<Canvas>();
        //     
        // foreach(GameObject card in clonedCards)
        // {
        //     // キャンバスにカードを追加
        //     card.transform.SetParent(canvas.transform);
        //
        //     // カードをボタンとして設定
        //     Button cardButton = card.GetComponent<Button>();
        //     cardButton.onClick.AddListener(() => ToggleCardSelection(card));
        // }

        Debug.Log("カードと数字を入力してください");

        while(!confirmchecked){
            yield return null;
        }

        //結果出力とUIの停止
        string cardname=selectCard.name;

        Debug.Log(cardname);
        int cardNumber=int.Parse(cardname[cardname.Length - 1].ToString());

        if(cardNumber>chooseNumber){
            //カードの値より入力した値が大きかった時
            Debug.Log($"カードの数値は{chooseNumber}より大きいです");
            networkSystem.Log($"カードの数値は{chooseNumber}より大きいです");
        }else{
            //入力した値がカードの値以下だった時
            Debug.Log($"カードの数値は{chooseNumber}以下です");
            networkSystem.Log($"カードの数値は{chooseNumber}以下です");
        }

        foreach (GameObject card in clonedCards) Destroy(card);

        selectCard=null;
        inputField.text="";
        nowUsingItem=-1;
        confirmchecked=false;
        ItemSixBG.SetActive(false);//非表示
        cardPanel.GameObject().SetActive(false);
    }

    void ToggleCardSelection(GameObject card){
        if(selectCard==card){
            selectCard=null;
            card.GetComponent<Image>().color = Color.white;
        }else{
            selectCard=card;
            foreach (GameObject othercard in clonedCards)othercard.GetComponent<Image>().color = Color.white;
            card.GetComponent<Image>().color = Color.red;
        }
    }

    void OnConfirmButtonClickedOne()
    {
        if(selectCard!=null && chooseCompareTo!=0){
            confirmchecked=true;
        }
        if(selectCard==null){
            Debug.Log("カードを選択してください");
        }
        if(chooseCompareTo==0){
            Debug.Log("大小どちらかを選択してください");
        }
    }

    void OnConfirmButtonClickedSix()
    {
        if(selectCard!=null && (chooseNumber>=1 && chooseNumber<=NetworkSystem.cardNum)){
            confirmchecked=true;
        }
        if(selectCard==null){
            Debug.Log("カードを選択してください");
        }
        if(chooseNumber<1 || chooseNumber>NetworkSystem.cardNum){
            Debug.Log($"1から{NetworkSystem.cardNum}までの数字を入力してください。");
        }
    }

    void OnSmallerButtonClicked()
    {
        chooseCompareTo=-1;
        smallerButton.GetComponent<Image>().color=Color.green;
        largerButton.GetComponent<Image>().color=Color.white;
    }

    void OnLargerButtonClicked()
    {
        chooseCompareTo=1;
        smallerButton.GetComponent<Image>().color=Color.white;
        largerButton.GetComponent<Image>().color=Color.green;
    }
}
