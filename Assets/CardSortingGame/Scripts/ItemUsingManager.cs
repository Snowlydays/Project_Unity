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
    private Transform itemSixCardPanel;
    private Transform itemOneCardPanel;

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

    public static Dictionary<int, string> itemNameDict = new Dictionary<int, string> {
        { 1, "オーブ" },
        { 2, "レンズ" },
        { 3, "ミラー" },
        { 4, "鎖" },
        { 5, "エリクサー" },
        { 6, "天秤" },
    };

    // パネルのサイズを調整する用の変数
    [SerializeField] private float cardWidth;       // 各カードの幅
    [SerializeField] private float cardSpacing;      // カード間のスペース
    [SerializeField] private float paddingLeft;     // パネルの左余白
    [SerializeField] private float paddingRight;    // パネルの右余白
    
    public AudioClip cardSound;
    public AudioClip confirmSound;
    public AudioClip decideSound;
    public AudioClip cancelSound;
    public GameObject SoundObject;

    void Start()
    {
        cardsManager = FindObjectOfType<CardsManager>();
        networkSystem = FindObjectOfType<NetworkSystem>();

        //各種UI初期化
        ItemSixBG = GameObject.Find("ItemSixBG");
        ItemOneBG = GameObject.Find("ItemOneBG");
        itemSixCardPanel = GameObject.Find("ItemSixCardPanel").transform;
        itemOneCardPanel = GameObject.Find("ItemOneCardPanel").transform;

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

        yield return StartCoroutine(ItemThreeCheckAndUse());//アイテム3効果を処理する部分

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

        yield return StartCoroutine(ItemFourCheckAndUse());//相手がアイテム4を使っているかどうかと実行処理

        allUsedItem=false;

        StartCoroutine(ItemUseAwaiting());

        yield return new WaitUntil(() => allUsedItem==true);

        Debug.Log("アイテムを全て使い切りました");

        allUsedItem=false;
        networkSystem.ToggleReady();
    }

    IEnumerator ItemFourCheckAndUse(){
        if(mylist.Contains(3) && otherlist.Contains(3)){
            networkSystem.questionController.isNotQuestion=true;
            networkSystem.animationController.CreateDefaultItem(3,-300f);
            networkSystem.animationController.CreateDefaultOtherItem(3,300f);
            yield return new WaitForSeconds(1.8f);
            networkSystem.informationManager.AddInformationText($"{itemNameDict[4]}の効果: 相手は質問できない");
            Debug.Log($"{itemNameDict[3]}の効果: 相手は質問できない");
            networkSystem.informationManager.AddInformationText($"{itemNameDict[4]}の効果: 自分の質問が封じられた。");
            Debug.Log($"{itemNameDict[3]}の効果: 自分の質問が封じられた。");
            yield break;
        }
        if(mylist.Contains(3)){
            networkSystem.animationController.CreateDefaultItem(3);
            yield return new WaitForSeconds(1.8f);
            networkSystem.informationManager.AddInformationText($"{itemNameDict[4]}の効果: 相手は質問できない");
            Debug.Log($"{itemNameDict[3]}の効果: 相手は質問できない");
            yield break;
        }
        if(otherlist.Contains(3)){
            //もし相手がアイテム4を持っていたら、質問不可能bool値をtrueにする。
            networkSystem.animationController.CreateDefaultOtherItem(3);
            networkSystem.questionController.isNotQuestion=true;
            yield return new WaitForSeconds(1.8f);
            networkSystem.informationManager.AddInformationText($"{itemNameDict[4]}の効果: 自分の質問が封じられた。");
            Debug.Log($"{itemNameDict[3]}の効果: 自分の質問が封じられた。");
            yield break;
        }
    }

    IEnumerator ItemUseAwaiting(){
        int lastitem=0;
        foreach(int item in mylist){
            nowUsingItem=item;
            Debug.Log($"{itemNameDict[item+1]}を使用しました");
            switch(item+1)
            {
                case 1:
                networkSystem.Log(1);
                break;
                case 2:
                networkSystem.Log(4);
                break;
                case 3:
                networkSystem.Log(3);
                break;
                case 4:
                networkSystem.Log(17);
                break;
                case 5:
                networkSystem.Log(0);
                break;
                case 6:
                networkSystem.Log(6);
                break;
            }
            if(item!=3)networkSystem.animationController.CreateDefaultItem(item);
            yield return new WaitForSeconds(1.8f);
            StartCoroutine(ApplyItemEffect(item));
            lastitem=item;
            //アイテム処理が終わったらnowUsingItem=-1;にして次のアイテム処理に移行させる。
            yield return new WaitUntil(() => nowUsingItem==-1);
        }
        if(lastitem==0 || lastitem==5)yield return new WaitForSeconds(1.5f);
        yield return new WaitForSeconds(1.5f);
        allUsedItem=true;
    }

    IEnumerator ItemThreeCheckAndUse(){
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
            networkSystem.informationManager.AddInformationText($"{itemNameDict[3]}の効果: アイテムを奪えなかった");
            Debug.Log($"{itemNameDict[3]}の効果: アイテムを奪えなかった");
            yield break;
        }
        
        if(mynum<=0 && otherusethree==true){
            //相手がアイテム3を使おうとしているのに自分がそもそもアイテムを使わない場合
            //リストからアイテム3をremoveして終了
            otherusethree=false;
            otherlist.Remove(2);
            yield break;
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
            if(mygetitem!=2){
                mylist[mylist.IndexOf(2)]=mygetitem;
                networkSystem.Log(3);//ログ適応
            }
            otherlist.Remove(mygetitem);
        }

        if(othergetitem!=-1){
            //相手も同様
            if(othergetitem!=2){
                otherlist[otherlist.IndexOf(2)]=othergetitem;
            }
            mylist.Remove(othergetitem);
        }

        if(mygetitem!=-1 & othergetitem==-1){
            //自分だけ使うパターン
            if(mygetitem!=2){
                networkSystem.animationController.CreateMyThreeItem(mygetitem);
                yield return new WaitForSeconds(3.5f);
                networkSystem.informationManager.AddInformationText($"{itemNameDict[3]}の効果: 相手の{itemNameDict[mygetitem]}を奪った。");
                Debug.Log($"{itemNameDict[3]}の効果: 相手の{itemNameDict[mygetitem]}を奪った。");
            }
        }

        if(mygetitem==-1 & othergetitem!=-1){
            //相手だけ使うパターン
            if(othergetitem!=2){
                networkSystem.animationController.CreateOtherThreeItem(othergetitem);
                yield return new WaitForSeconds(3.5f);
                networkSystem.informationManager.AddInformationText($"{itemNameDict[3]}の効果: 相手に{itemNameDict[othergetitem]}を奪われた。");
                Debug.Log($"{itemNameDict[3]}の効果: 相手に{itemNameDict[othergetitem]}を奪われた。");
            }
        }

        if(mygetitem!=-1 & othergetitem!=-1){
            //両方使うパターン
            if(mygetitem!=2 & othergetitem!=2){
                networkSystem.animationController.CreateMyThreeItem(mygetitem,-300f);
                networkSystem.animationController.CreateOtherThreeItem(othergetitem,300f);
                yield return new WaitForSeconds(3.5f);
                networkSystem.informationManager.AddInformationText($"{itemNameDict[3]}の効果: 相手の{itemNameDict[mygetitem]}を奪った。");
                Debug.Log($"{itemNameDict[3]}の効果: 相手の{itemNameDict[mygetitem]}を奪った。");
                networkSystem.informationManager.AddInformationText($"{itemNameDict[3]}の効果: 相手に{itemNameDict[othergetitem]}を奪われた。");
                Debug.Log($"{itemNameDict[3]}の効果: 相手に{itemNameDict[othergetitem]}を奪われた。");
            }
        }

        if(mygetitem==-1 || mygetitem==2){
            networkSystem.informationManager.AddInformationText($"{itemNameDict[3]}の効果: アイテムを奪えなかった");
            Debug.Log($"{itemNameDict[3]}の効果: アイテムを奪えなかった");
        }

        //アイテム3アニメーションから3.5秒後にログ表示、ログを1.5秒程度見せてから次へ
        if(mygetitem!=-1 || othergetitem!=-1)yield return new WaitForSeconds(1.5f);
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

    IEnumerator ApplyItemEffect(int itemIdx)
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
                networkSystem.informationManager.AddInformationText($"{itemNameDict[2]}の効果: 次比較するカードの差がわかる");
                Debug.Log($"{itemNameDict[2]}の効果: 次比較するカードの差がわかる");
            break;

            //2,3は外部処理のためここには記述なし

            case 4:
                networkSystem.questionController.isThreeSelect=true;
                nowUsingItem=-1;
                networkSystem.informationManager.AddInformationText($"{itemNameDict[5]}の効果: 次の質問でカードを3つ比較する");
                Debug.Log($"{itemNameDict[5]}の効果: 次の質問でカードを3つ比較する");
            break;

            case 5:
                StartCoroutine(ItemUseSix());
            break;

            default:
                nowUsingItem=-1;
            break;
        }

        yield break;
    }

    IEnumerator ItemUseOne()
    {
        //各種UIの起動
        ItemOneBG.SetActive(true);//起動
        clonedCards = cardsManager.PlaceCardsOnPanel(itemOneCardPanel,ToggleCardSelection, cardWidth, cardSpacing, paddingLeft, paddingRight);

        chooseCompareTo=0;
        smallerButton.GetComponent<Image>().color=Color.white;
        largerButton.GetComponent<Image>().color=Color.white;
        
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

        // 初期インデックスを保存
        int initialIndex = startIndex;
        
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

        // 最終インデックスを保存
        int finalIndex = startIndex;

        // 移動量を計算
        int distanceMoved = finalIndex - initialIndex;

        // 移動方向と移動量をinfoTextでプレイヤーへ通知
        if(distanceMoved != 0)
        {
            networkSystem.informationManager.AddInformationText($"{itemNameDict[1]}の効果: 選んだ{initialIndex + 1}番目のカードは{finalIndex + 1}番目まで移動しました。");
            Debug.Log($"{itemNameDict[1]}の効果: 選んだ{initialIndex + 1}番目のカードは{finalIndex + 1}番目まで移動しました。");
        }
        else
        {
            networkSystem.informationManager.AddInformationText($"{itemNameDict[1]}の効果: 選んだ{initialIndex + 1}番目のカードは移動しませんでした。");
            Debug.Log($"{itemNameDict[1]}の効果: 選んだ{initialIndex + 1}番目のカードは移動しませんでした。");
        }
        
        foreach (GameObject card in clonedCards) Destroy(card);

        chooseCompareTo=0;
        selectCard=null;
        nowUsingItem=-1;
        confirmchecked=false;
        ItemOneBG.SetActive(false);
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
        cardsManager.myCards[fromIndex].cardObject.transform.SetParent(toSlot, false);
        cardsManager.myCards[fromIndex].cardObject.transform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        //配列の並び替え
        cardsManager.SwapCardsInList(toIndex, fromIndex);
    }

    IEnumerator ItemUseSix()
    {
        //各種UIの起動
        ItemSixBG.SetActive(true);//起動
        clonedCards = cardsManager.PlaceCardsOnPanel(itemSixCardPanel,ToggleCardSelection, cardWidth, cardSpacing, paddingLeft, paddingRight);
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
            Debug.Log($"{itemNameDict[6]}の効果:カードの数値は{chooseNumber}より大きいです");
            networkSystem.Log(7);
            networkSystem.informationManager.AddInformationText($"{itemNameDict[6]}の効果:選んだカードの数値は{chooseNumber}より大きいです");
        }else{
            //入力した値がカードの値以下だった時
            Debug.Log($"{itemNameDict[6]}の効果:カードの数値は{chooseNumber}以下です");
            networkSystem.Log(8);
            networkSystem.informationManager.AddInformationText($"{itemNameDict[6]}の効果:選んだカードの数値は{chooseNumber}以下です");
        }

        foreach (GameObject card in clonedCards) Destroy(card);

        selectCard=null;
        inputField.text="";
        nowUsingItem=-1;
        confirmchecked=false;
        ItemSixBG.SetActive(false);//非表示
    }

    void ToggleCardSelection(GameObject card){
        if(selectCard==card){
            GameObject soundobj=Instantiate(SoundObject);
            soundobj.GetComponent<PlaySound>().PlaySE(cancelSound);
            selectCard=null;
            cardsManager.DeselectCardUI(card); // カードを下げる
            // card.GetComponent<Image>().color = Color.white;
        }else{
            GameObject soundobj=Instantiate(SoundObject);
            soundobj.GetComponent<PlaySound>().PlaySE(cardSound);
            if(selectCard!=null){
                // selectCard.GetComponent<Image>().color = Color.white;
                cardsManager.DeselectCardUI(selectCard); // 元々選んでいたカードを下げる
            }
            selectCard = card;
            cardsManager.SelectCardUI(card); // 新たに選んだカードをあげる
            // card.GetComponent<Image>().color = Color.red;
            // foreach (GameObject othercard in clonedCards)othercard.GetComponent<Image>().color = Color.white;
        }
    }

    void OnConfirmButtonClickedOne()
    {
        if(selectCard!=null && chooseCompareTo!=0){
            GameObject soundobj=Instantiate(SoundObject);
            soundobj.GetComponent<PlaySound>().PlaySE(confirmSound);
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
            GameObject soundobj=Instantiate(SoundObject);
            soundobj.GetComponent<PlaySound>().PlaySE(confirmSound);
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
        GameObject soundobj=Instantiate(SoundObject);
        soundobj.GetComponent<PlaySound>().PlaySE(decideSound);
        chooseCompareTo=-1;
        smallerButton.GetComponent<Image>().color=Color.green;
        largerButton.GetComponent<Image>().color=Color.white;
    }

    void OnLargerButtonClicked()
    {
        GameObject soundobj=Instantiate(SoundObject);
        soundobj.GetComponent<PlaySound>().PlaySE(decideSound);
        chooseCompareTo=1;
        smallerButton.GetComponent<Image>().color=Color.white;
        largerButton.GetComponent<Image>().color=Color.green;
    }
}