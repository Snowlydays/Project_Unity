using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class MainSystemScript : MonoBehaviour
{
    private NetworkSystem networkSystem;
    private CardsManager cardsManager;

    private static readonly int CARD_NUM = NetworkSystem.cardNum;
    private GameObject[] mycard = new GameObject[CARD_NUM];//自分の手札
    private GameObject[] othercard = new GameObject[CARD_NUM];//相手の手札
    private GameObject[] mySlots = new GameObject[CARD_NUM]; // スロットを保持する配列
    private GameObject[] otherSlots = new GameObject[CARD_NUM]; // スロットを保持する配列
    
    [SerializeField] public Button readyButton; // 準備完了ボタン
    [SerializeField] public Transform myCardPanel; // 自分のカードを配置するパネル
    [SerializeField] private Transform otherCardPanel; // 相手のカードを配置するパネル
    [SerializeField] public GameObject CardPrefab; // カードのプレハブ
    [SerializeField] public GameObject SlotPrefab; // スロットのプレハブ

    private Image buttonImage;                         // ボタンのImageコンポーネント
    [SerializeField] private Sprite readySprite;        // 準備完了時のスプライト
    [SerializeField] private Sprite notReadySprite;     // 未準備時のスプライト

    private const int PHASE_NUM = 4;
    private const int attackGuideID = 3; // MainSystemオブジェクトのGuideSprites/Element3が攻撃時のガイド
    [SerializeField] private Image guideImage; // ガイドを表示するimageオブジェクト
    [SerializeField] private Sprite[] guideSprites = new Sprite[PHASE_NUM]; // ガイド用の画像を保存する配列

    public AudioClip confirmSound;
    public GameObject SoundObject;
    
    [SerializeField] private Sprite[] numberSprites = new Sprite[10]; // 数字のスプライト(0-9まで)
    public int[] otherCardNumber = new int[CARD_NUM];

    public GameObject[] GetMyCards()
    {
        return mycard;
    }

    public GameObject[] GetmySlots()
    {
        return mySlots;
    }

    void Awake(){
        networkSystem = FindObjectOfType<NetworkSystem>();
        cardsManager = FindObjectOfType<CardsManager>();
    }

    // パネルのサイズを調整する用の変数
    [SerializeField] private float cardWidth;       // 各カードの幅
    [SerializeField] private float cardSpacing;      // カード間のスペース
    [SerializeField] private float paddingLeft;     // パネルの左余白
    [SerializeField] private float paddingRight;    // パネルの右余白
    
    void Start()
    {
        Debug.Log("ゲームスタート");
        // ボタンのImageコンポーネントを取得
        buttonImage = readyButton.GetComponent<Image>();
    
        // 初期状態の画像を設定
        UpdateButtonImage(false);
        if(networkSystem != null)networkSystem.OnLocalReadyStateChanged += UpdateButtonImage;
        
        // パネルのサイズ調整
        RectTransform myCardPanelRect = myCardPanel.GetComponent<RectTransform>();
        RectTransform otherCardPanelRect = otherCardPanel.GetComponent<RectTransform>();
        cardsManager.AdjustPanelSize(myCardPanelRect,cardWidth,cardSpacing,paddingLeft,paddingRight);
        cardsManager.AdjustPanelSize(otherCardPanelRect,cardWidth,cardSpacing,paddingLeft,paddingRight);
        
        // 自分のスロット、カード生成
        for (int i = 0; i < CARD_NUM; i++)
        {
            // スロット生成
            mySlots[i] = Instantiate(SlotPrefab, myCardPanel);
            CardSlot myCardSlot = mySlots[i].GetComponent<CardSlot>();
            myCardSlot.isMySlot = true;

            // スロット内にカードを配置
            mycard[i] = Instantiate(CardPrefab, mySlots[i].transform);
            mycard[i].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            
            // DraggableCard スクリプトを取得してドラッグ"可能"に設定
            DraggableCard draggable = mycard[i].GetComponent<DraggableCard>();
            draggable.isDraggable = true;
        }

        // 相手のカード生成
        for (int i = 0; i < CARD_NUM; i++)
        {
            // スロット生成
            otherSlots[i] = Instantiate(SlotPrefab, otherCardPanel);
            CardSlot otherCardSlot = otherSlots[i].GetComponent<CardSlot>();
            otherCardSlot.isMySlot = false;
            
            // カードを配置
            othercard[i] = Instantiate(CardPrefab, otherSlots[i].transform);
            othercard[i].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            // DraggableCard スクリプトを取得してドラッグ"不可"に設定
            DraggableCard draggable = othercard[i].GetComponent<DraggableCard>();
            draggable.isDraggable = false;
        }
        
        readyButton.onClick.AddListener(OnReadyButtonClicked); // readyボタンにリスナーを追加
    }

    public void UpdateOtherCardUI()
    {
        for (int i = 0; i < CARD_NUM; i++)
        {
            Image cardsImage = othercard[i].GetComponent<Image>();
            cardsImage.sprite = numberSprites[otherCardNumber[i]];
        }
    }
    public void ChangeGuideImage(int phase, bool isAttack=false)
    {
        int id = (isAttack ? attackGuideID : phase); // 攻撃中ならば、そのガイドを表示
        if (phase == NetworkSystem.itemUsingPhase) id = NetworkSystem.itemPhase; // ItemUsingPhaseの場合は、itemPhaseと同じGuideを表示
        guideImage.sprite = guideSprites[id];
    }

    
    void OnReadyButtonClicked()
    {
        //phaseが0以外の時は機能しないように制限
        //演出中とかに押されると勝手に次のフェーズにいかれる恐れがあるため
        Debug.Log("readyButton clicked");
        if(NetworkSystem.phase==0){
            networkSystem.ToggleReady();
            readyButton.gameObject.GetComponent<Animator>().SetBool("blStarted", true);
            GameObject soundobj=Instantiate(SoundObject);
            soundobj.GetComponent<PlaySound>().PlaySE(confirmSound);
        }
    }
    
    // 準備状態に基づいてボタンの画像を更新
    private void UpdateButtonImage(bool isReady)
    {
        if (buttonImage != null)
            buttonImage.sprite = isReady ? readySprite : notReadySprite;
    }

    void Update()
    {
        //メインカメラを取得し、フェーズによってその背景色を変更する。
        //フェーズの取得はNetworkSystemが管理するphase変数から参照する。
        if(NetworkSystem.phase==2){
            Camera.main.backgroundColor = new Color(255f, 152f/255f, 226f/255f);
        }else{
            Camera.main.backgroundColor = new Color(1f, 1f, 1f);
        }
    }
}
