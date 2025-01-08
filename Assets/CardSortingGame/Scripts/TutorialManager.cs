using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    private bool isFirstTutorialClosed = false;
    private int currentShowTutorialNumber = 0;
    private const int maxPhaseNum=5;
    [SerializeField] private int[] TutorialPageSizes;
    public int tutorialPhase=0;

    [SerializeField] private Transform tutorialPage;
    [SerializeField] private Transform[] tutorialPageObjects0;
    [SerializeField] private Transform[] tutorialPageObjects1;
    [SerializeField] private Transform[] tutorialPageObjects2;
    [SerializeField] private Transform[] tutorialPageObjects3;
    [SerializeField] private Transform[] tutorialPageObjects4;
    [SerializeField] private Transform[] tutorialPageObjects5;

    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button showTutorialButton;
    [SerializeField] private Button closeTutorialButton;
    [SerializeField] private Button endTutorialButton;

    [SerializeField] private Sprite prevDisabledSprite;
    [SerializeField] private Sprite prevEnabledSprite;
    [SerializeField] private Sprite nextDisabledSprite;
    [SerializeField] private Sprite nextEnabledSprite;
    //[SerializeField] private Sprite[] tutorialSprites = new Sprite[TutorialPageSize];

    private CardsManager cardsManager;

    [SerializeField] private GameObject[] unMasks;

    [SerializeField] private float maskWidth;
    [SerializeField] private float maskSpacing;
    [SerializeField] private float paddingLeft;
    [SerializeField] private float paddingRight;

    public bool nowTutorialViewing;

    //リアルタイムでプレイしながらチュートリアルを表示していく形式に変更。
    //左上のバツボタンを2回押すことでチュートリアル自体のスキップが可能
    
    private void Awake()
    {
        isFirstTutorialClosed = false;
        prevButton.onClick.AddListener(OnPrevButtonClicked);
        nextButton.onClick.AddListener(OnNextButtonClicked);
        showTutorialButton.onClick.AddListener(OnShowTutorialButtonClicked);
        closeTutorialButton.onClick.AddListener(OnCloseTutorialButtonClicked);
        endTutorialButton.onClick.AddListener(OnEndTutorialButtonClicked);

        ResetTutorialPage();
        nowTutorialViewing=true;
    }

    private void Start(){
        cardsManager = FindObjectOfType<CardsManager>();
        cardsManager.AdjustPanelSize(unMasks[0].GetComponent<RectTransform>(),maskWidth,maskSpacing,paddingLeft,paddingRight);
        unMasks[0].GetComponent<RectTransform>().sizeDelta+= new Vector2(40f,0f);
        cardsManager.AdjustPanelSize(unMasks[1].GetComponent<RectTransform>(),maskWidth,maskSpacing,paddingLeft,paddingRight);
        unMasks[1].GetComponent<RectTransform>().localScale = new Vector3(0.91f, 0.91f, 1f);
        unMasks[1].GetComponent<RectTransform>().sizeDelta+= new Vector2(40f,0f);
        cardsManager.AdjustPanelSize(unMasks[2].GetComponent<RectTransform>(),maskWidth,maskSpacing,paddingLeft,paddingRight);
        unMasks[2].GetComponent<RectTransform>().sizeDelta+= new Vector2(40f,0f);
        cardsManager.AdjustPanelSize(unMasks[3].GetComponent<RectTransform>(),maskWidth,maskSpacing,paddingLeft,paddingRight);
        unMasks[3].GetComponent<RectTransform>().sizeDelta+= new Vector2(40f,0f);
        cardsManager.AdjustPanelSize(unMasks[4].GetComponent<RectTransform>(),maskWidth,maskSpacing,paddingLeft,paddingRight);
        unMasks[4].GetComponent<RectTransform>().sizeDelta+= new Vector2(40f,0f);
        float panelwidth=unMasks[4].GetComponent<RectTransform>().sizeDelta.x-40f;
        unMasks[5].GetComponent<RectTransform>().anchoredPosition=new Vector3(
        -panelwidth/2f+101.25f,
        -39f,0f);
        unMasks[6].GetComponent<HandAnimScript>().SetPosition(
        -panelwidth/2f+101.25f+53f,
        -90f
        );
    }
    
    private void OnNextButtonClicked()
    {
        if (currentShowTutorialNumber < TutorialPageSizes[tutorialPhase]-1)
        {
            currentShowTutorialNumber++;
            UpdateTutorialUI();
            SoundManager.PlaySEnum(3);
        }
    }

    private void OnPrevButtonClicked()
    {
        if (currentShowTutorialNumber > 0)
        {
            currentShowTutorialNumber--;
            UpdateTutorialUI();
            SoundManager.PlaySEnum(3);
        }
    }

    private void OnShowTutorialButtonClicked()
    {
        tutorialPage.gameObject.SetActive(true);
        endTutorialButton.gameObject.SetActive(true);
        //tutorialPhase=0;
        nowTutorialViewing=true;
        SoundManager.PlaySEnum(2);
    }

    public void ShowTutorial(){
        if(nowTutorialViewing==true){
            tutorialPage.gameObject.SetActive(true);
            SoundManager.PlaySEnum(2);
        }
    }

    private void OnCloseTutorialButtonClicked()
    {
        if (!isFirstTutorialClosed)
        {
            FindObjectOfType<TimerController>().StartDrawBar();
            isFirstTutorialClosed = true;
        }
        if(nowTutorialViewing==false || tutorialPhase>=maxPhaseNum){
            ResetTutorialPage();
            endTutorialButton.gameObject.SetActive(false);
        }else{
            currentShowTutorialNumber=0;
        }
        AllDeactive();
        UpdateTutorialUI();
        tutorialPage.gameObject.SetActive(false);
        SoundManager.PlaySEnum(0);
    }

    public void OnEndTutorialButtonClicked()
    {
        //実行中のチュートリアルを強制終了する
        if(nowTutorialViewing==true){
            endTutorialButton.gameObject.SetActive(false);
            ResetTutorialPage();
            SoundManager.PlaySEnum(0);
        }
    }

    private void AllDeactive(){
        foreach (Transform obj in tutorialPageObjects0) obj.gameObject.SetActive(false);
        foreach (Transform obj in tutorialPageObjects1) obj.gameObject.SetActive(false);
        foreach (Transform obj in tutorialPageObjects2) obj.gameObject.SetActive(false);
        foreach (Transform obj in tutorialPageObjects3) obj.gameObject.SetActive(false);
        foreach (Transform obj in tutorialPageObjects4) obj.gameObject.SetActive(false);
        foreach (Transform obj in tutorialPageObjects5) obj.gameObject.SetActive(false);
    }
    private void UpdateTutorialUI()
    {
        //Image tutorialPageImage = tutorialPage.GetComponent<Image>();
        Image prevImage = prevButton.GetComponent<Image>();
        Image nextImage = nextButton.GetComponent<Image>();
        //tutorialPageImage.sprite = tutorialSprites[currentShowTutorialNumber];
        prevImage.sprite = (currentShowTutorialNumber <= 0 ? prevDisabledSprite : prevEnabledSprite);
        nextImage.sprite = (currentShowTutorialNumber >= TutorialPageSizes[tutorialPhase]-1 ? nextDisabledSprite : nextEnabledSprite);

        switch(tutorialPhase){
            case 0:
                foreach (Transform obj in tutorialPageObjects0) obj.gameObject.SetActive(false);
                tutorialPageObjects0[currentShowTutorialNumber].gameObject.SetActive(true);
            break;
            case 1:
                foreach (Transform obj in tutorialPageObjects1) obj.gameObject.SetActive(false);
                tutorialPageObjects1[currentShowTutorialNumber].gameObject.SetActive(true);
            break;
            case 2:
                foreach (Transform obj in tutorialPageObjects2) obj.gameObject.SetActive(false);
                tutorialPageObjects2[currentShowTutorialNumber].gameObject.SetActive(true);
            break;
            case 3:
                foreach (Transform obj in tutorialPageObjects3) obj.gameObject.SetActive(false);
                tutorialPageObjects3[currentShowTutorialNumber].gameObject.SetActive(true);
            break;
            case 4:
                foreach (Transform obj in tutorialPageObjects4) obj.gameObject.SetActive(false);
                tutorialPageObjects4[currentShowTutorialNumber].gameObject.SetActive(true);
            break;
            case 5:
                foreach (Transform obj in tutorialPageObjects5) obj.gameObject.SetActive(false);
                tutorialPageObjects5[currentShowTutorialNumber].gameObject.SetActive(true);
            break;
        }
    }
    
    private void ResetTutorialPage()
    {
        AllDeactive();
        currentShowTutorialNumber = 0;
        nowTutorialViewing=false;
        UpdateTutorialUI();
    }

    public void ChangeTutorialPhase(int phase){
        AllDeactive();
        currentShowTutorialNumber = 0;
        tutorialPhase=phase;
        UpdateTutorialUI();
    }
}