using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.VisualScripting;
using TMPro;

public class QutstionController : MonoBehaviour
{
    GameObject questionBG;
    private Transform cardPanel; // UIカードを配置するパネル
    
    private List<GameObject> selectedCards = new List<GameObject>();  // 選択されたカードのリスト
    private Color originalColor = Color.white;  // デフォルトのカードの色
    private Color selectedColor = Color.yellow; // 選択されたカードの色

    [SerializeField] private Button confirmButton; // 決定ボタン
    [SerializeField] private Button spellButton; // スペルボタン
    private bool isAttacking = false;
    
    private CardsManager cardsManager;
    private NetworkSystem networkSystem;

    public bool isGetDiff = false;
    public bool isThreeSelect = false;
    public bool isNotQuestion = false;
    
    private TextMeshProUGUI instruction;
    
    [SerializeField] private Sprite questionQuestionBGSprite;  // 通常時のQuestionBGのスプライト
    [SerializeField] private Sprite attackingQuestionBGSprite; // 攻撃時のQuestionBGのスプライト

    [SerializeField] private Sprite questionSpellButtonSprite;  // 通常時のSpellButtonのスプライト
    [SerializeField] private Sprite attackingSpellButtonSprite; // 攻撃時のSpellButtonのスプライト

    private Image questionBGImage;    // QuestionBGのImage
    private Image spellButtonImage;   // SpellButtonのImage
    
    // パネルのサイズを調整する用の変数
    [SerializeField] private float cardWidth;       // 各カードの幅
    [SerializeField] private float cardSpacing;      // カード間のスペース
    [SerializeField] private float paddingLeft;     // パネルの左余白
    [SerializeField] private float paddingRight;    // パネルの右余白

    public AudioClip cardSound;
    public GameObject cardSoundObject;

    void Start()
    {
        cardsManager = FindObjectOfType<CardsManager>();
        networkSystem = FindObjectOfType<NetworkSystem>();
        
        questionBG = GameObject.Find("QuestioningBG");
        cardPanel = GameObject.Find("QuestionCardPanel").transform;
        instruction = GameObject.Find("Text").GetComponent<TextMeshProUGUI>(); // 案内テキストを取得
        questionBG.SetActive(false);// 非表示
        
        // Imageコンポーネントを取得
        questionBGImage = questionBG.GetComponent<Image>();
        spellButtonImage = spellButton.GetComponent<Image>();
        
        confirmButton.GetComponent<Animator>().keepAnimatorStateOnDisable = true;
        
        // クリックイベント設定
        confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
        spellButton.onClick.AddListener(OnSpellButtonClicked);
    }

    public void StartQuestionPhase()
    {
        instruction.text = isThreeSelect ? "3" : "2"; // 案内テキストの中身を変更
        if(!isNotQuestion){
            questionBG.SetActive(true);
            confirmButton.gameObject.SetActive(false);
            
            // 通常時のスプライトに初期化
            questionBGImage.sprite = questionQuestionBGSprite;
            spellButtonImage.sprite = questionSpellButtonSprite;
            
            cardPanel.GameObject().SetActive(true);
            cardsManager.PlaceCardsOnPanel(cardPanel,ToggleCardSelection, cardWidth, cardSpacing, paddingLeft, paddingRight);
        }else{
            //質問ができない場合はアイテム効果系bool変数を無効化してToggleReadyする。
            Debug.Log("相手のアイテム4の効果で質問ができない");
            networkSystem.informationManager.AddInformationText($"相手の{ItemUsingManager.itemNameDict[4]}の効果により質問できません!");
            // 攻撃トグルを初期化
            isAttacking = false;

            // 3枚選択トグルを初期化
            isThreeSelect=false;

            // 質問不可能トグルを初期化
            isNotQuestion=false;

            // 通常フェーズへ戻るためにreadyをトグルする
            networkSystem.ToggleReady();
        }
    }

    void OnSpellButtonClicked()
    {
        Debug.Log("スペルボタン クリック");
        isAttacking = !isAttacking;
        networkSystem.ToggleAttacked();

        // 背面の変更
        if (isAttacking)
        {
            // 攻撃時のスプライトに変更
            instruction.gameObject.SetActive(false);
            questionBGImage.sprite = attackingQuestionBGSprite;
            spellButtonImage.sprite = attackingSpellButtonSprite;
        }
        else
        {
            // 通常時のスプライトに戻す
            instruction.gameObject.SetActive(true);
            questionBGImage.sprite = questionQuestionBGSprite;
            spellButtonImage.sprite = questionSpellButtonSprite;
        }
        
        // ガイドの変更
        networkSystem.mainSystemScript.ChangeGuideImage(NetworkSystem.phase, isAttacking);
    }
    
    // カード選択状態の切り替え関数
    void ToggleCardSelection(GameObject card)
    {
        // カードが既に選択されている場合、選択を解除する
        if(selectedCards.Contains(card))
        {
            DeselectCard(card);
        }
        else
        {
            //2枚選択か3枚選択かで分ける
            if(!isThreeSelect)
            {
                // 2枚を超えた選択は許可しない
                if(selectedCards.Count < 2)SelectCard(card);
            }else{
                // 3枚を超えた選択は許可しない
                if(selectedCards.Count < 3)SelectCard(card);
            }
        }
    }

    void SelectCard(GameObject card)
    {
        // カードを選択状態にし、色を変更
        GameObject soundobj=Instantiate(cardSoundObject);
        soundobj.GetComponent<PlaySound>().PlaySE(cardSound);
        
        selectedCards.Add(card);
        cardsManager.SelectCardUI(card);
        card.GetComponent<Image>().color = selectedColor;  // 色を変更
        Debug.Log("選択したカード枚数: "+selectedCards.Count);
    }
    
    void DeselectCard(GameObject card)
    {
        // カードの選択を解除し、色を元に戻す
        selectedCards.Remove(card);
        cardsManager.DeselectCardUI(card);
        card.GetComponent<Image>().color = originalColor;  // 元の色に戻す
    }

    void Update(){
        if(questionBG.activeSelf){
            if(confirmButton.gameObject.activeSelf)confirmButton.gameObject.SetActive(false);
            if(isThreeSelect){
                if (selectedCards.Count == 3)
                {
                    if(!confirmButton.gameObject.activeSelf)confirmButton.gameObject.SetActive(true);
                    return;
                }
            }else{
                if (selectedCards.Count == 2)
                {
                    if(!confirmButton.gameObject.activeSelf)confirmButton.gameObject.SetActive(true);
                    return;
                }
            }
            if (isAttacking){
                if(!confirmButton.gameObject.activeSelf)confirmButton.gameObject.SetActive(true);
                return;
            }
        }
    }
    
    void OnConfirmButtonClicked()
    {
        if (isAttacking || selectedCards.Count >= 2)
        {
            if (isAttacking) Debug.Log("QuestionController: 攻撃します");
            else if(!isThreeSelect)
            {
                //2枚選択の場合
                int ans = -1;
                if (selectedCards.Count == 2)
                {
                    ans = CompareCards(selectedCards[0], selectedCards[1]);
                }
            }else{
                //三枚選択の場合
                int ans = -1;
                if (selectedCards.Count == 3)
                {
                    ans = CompareThreeCards(selectedCards[0], selectedCards[1], selectedCards[2]);
                }else{
                    Debug.Log("Please select exactly three cards to compare.");
                    return;
                }
            }
            // 比較後に選択状態をリセット
            foreach (GameObject card in selectedCards)
            {
                card.GetComponent<Image>().color = originalColor; // 色を元に戻す
            }
            selectedCards.Clear();

            // クローンカードのUIを削除
            GameObject[] clonedCards = GameObject.FindGameObjectsWithTag("ClonedCard");
            foreach (GameObject card in clonedCards) Destroy(card);

            // 背景のパネルを非表示に
            instruction.gameObject.SetActive(true); // 攻撃時にsetActiveがfalseになっている場合でも、初期値に戻す
            questionBG.SetActive(false);
            cardPanel.GameObject().SetActive(false);
            
            // 攻撃トグルを初期化
            isAttacking = false;

            // 3枚選択トグルを初期化
            isThreeSelect=false;

            // 通常フェーズへ戻るためにreadyをトグルする
            networkSystem.ToggleReady();
        }
        else
        {
            Debug.Log("Please select exactly two cards to compare.");
        }
    }
    
    int CompareCards(GameObject leftCard, GameObject rightCard)
    {
        if(leftCard.transform.position.x > rightCard.transform.position.x)
        {
            (leftCard, rightCard) = (rightCard, leftCard);
        }
        
        string leftName = leftCard.name, rightName = rightCard.name;
        Debug.Log("left:"+leftName + " right:"+rightName);

        //ここの処理の補足
        //これで得られるのは数値ではなく、厳密には「先頭文字を文字コードで表したときの数値」
        //文字コード表では基本的に1から9の順に文字コードの数値も大きくなるので、一応これでも成立する
        string informationText = "";
        if(leftName[leftName.Length - 1] < rightName[rightName.Length - 1]) informationText = "右のカードの方が大きい";
        else informationText = "左のカードの方が大きい";
        
        if(isGetDiff){
            //アイテム2の処理
            int Diff=0;
            Diff=Mathf.Abs(leftName[leftName.Length - 1] - rightName[rightName.Length - 1]);

            Debug.Log($"{ItemUsingManager.itemNameDict[2]}の効果:カードの差は"+Diff.ToString()+"です");
            networkSystem.Log($"{ItemUsingManager.itemNameDict[2]}の効果:カードの差は"+Diff.ToString()+"です");
            informationText = informationText + $"\n{ItemUsingManager.itemNameDict[2]}の効果:カードの差は" + Diff.ToString() + "です";

            isGetDiff=false;
        }
        networkSystem.informationManager.AddQuestionResult(informationText);
        networkSystem.Log(informationText);
        return 0;
    }

    int CompareThreeCards(GameObject leftCard, GameObject middleCard, GameObject rightCard)
    {
        GameObject[] Cards ={leftCard,middleCard,rightCard};

        Cards.OrderBy(e => e.transform.position.x);//x座標の昇順に並び替え 仮
        
        string leftName = Cards[0].name, middleName = Cards[1].name, rightName = Cards[2].name;
        Debug.Log("left:"+leftName + " middle:"+middleName + " right:"+rightName);
        
        int[] array={leftName[leftName.Length - 1],middleName[middleName.Length - 1],rightName[rightName.Length - 1]};
        
        string left="",middle="",right="";

        int maxindex=Array.IndexOf(array,array.Max());

        switch(maxindex)
        {
            case 0:
                left="big";
            break;
            case 1:
                middle="big";
            break;
            case 2:
                right="big";
            break;
        }

        int minindex=Array.IndexOf(array,array.Min());

        switch(minindex)
        {
            case 0:
                left="small";
            break;
            case 1:
                middle="small";
            break;
            case 2:
                right="small";
            break;
        }

        if(left==""){
            left="middle";
        }else if(middle==""){
            middle="middle";
        }else{
            right="middle";
        }

        Debug.Log($"{ItemUsingManager.itemNameDict[5]}の効果:"+"Left:"+left+" middle:"+middle+" right:"+right);
        networkSystem.Log($"{ItemUsingManager.itemNameDict[5]}の効果:"+"左:"+left+" 中:"+middle+" 右:"+right);
        networkSystem.informationManager.AddQuestionResult($"{ItemUsingManager.itemNameDict[5]}の効果:"+"左:"+left+" 中:"+middle+" 右:"+right);
        return 0;
    }
}
