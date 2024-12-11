using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public enum TabType { All, Myself, Opponent, Possesed }

public class LogMenuController : MonoBehaviour
{
    public Button myTabButton, opponentTabButton, closeButton;
    public List<LogUnit> allLogs = new List<LogUnit>();
    public List<LogUnit> myLogs = new List<LogUnit>();
    public List<LogUnit> opponentLogs = new List<LogUnit>();

    [SerializeField] public GameObject allLogMenu;
    [SerializeField] public GameObject myLogMenu;
    [SerializeField] public GameObject oppLogMenu;
    [SerializeField] public GameObject myLogPrefab;
    [SerializeField] public GameObject opponentLogPrefab;
    [SerializeField] public GameObject roundLogPrefab;
    
    private TabType currentTab = TabType.All;

    [SerializeField] private RectTransform drawerPanel;
    [SerializeField] private float animationDuration = 0.3f; // アニメーションの時間

    [SerializeField] private Sprite myButtonSprite;
    [SerializeField] private Sprite opponentButtonSprite;
    [SerializeField] private Sprite pushedMyButtonSprite;
    [SerializeField] private Sprite pushedOpponentButtonSprite;

    private Image myButtonImage;
    private Image opponentButtonImage;

    private RectTransform myButtonTransform;
    private RectTransform opponentButtonTransform;
    [SerializeField] private ScrollRect scrollRect;

    private Vector2 baseSize = new Vector2(646, 414);
    private Vector2 pushedSize = new Vector2(646, 375);

    public bool activeButton = true;
    private bool myButtonPushed = false;
    private bool opponentButtonPushed = false;

    private NetworkSystem networkSystem;

    void Start()
    {
        myLogMenu.SetActive(false);
        oppLogMenu.SetActive(false);
        allLogMenu.SetActive(true);

        myButtonImage = myTabButton.GetComponent<Image>();
        opponentButtonImage = opponentTabButton.GetComponent<Image>();

        myButtonTransform = myTabButton.GetComponent<RectTransform>();
        opponentButtonTransform = opponentTabButton.GetComponent<RectTransform>();
        
        myTabButton.onClick.AddListener(() => ManageTabState(TabType.Myself));
        opponentTabButton.onClick.AddListener(() => ManageTabState(TabType.Opponent));
        closeButton.onClick.AddListener(CloseDrawer);

        drawerPanel.anchoredPosition = new Vector2(-drawerPanel.rect.width, drawerPanel.anchoredPosition.y);
        networkSystem = FindObjectOfType<NetworkSystem>();
        // Debug();
    }

    private void ManageTabState(TabType pushedButton)
    {
        if(pushedButton == TabType.Myself)
        {
            myButtonPushed = !myButtonPushed;
            opponentButtonPushed = false;
        }
        else if(pushedButton == TabType.Opponent)
        {
            opponentButtonPushed = !opponentButtonPushed;
            myButtonPushed = false;
        }

        if(myButtonPushed)
        {
            myButtonImage.sprite = pushedMyButtonSprite;
            myButtonTransform.sizeDelta = pushedSize;
        }
        else
        {
            myButtonImage.sprite = myButtonSprite;
            myButtonTransform.sizeDelta = baseSize;
        }
        if(opponentButtonPushed)
        {
            opponentButtonImage.sprite = pushedOpponentButtonSprite;
            opponentButtonTransform.sizeDelta = pushedSize;
        }
        else
        {
            opponentButtonImage.sprite = opponentButtonSprite;
            opponentButtonTransform.sizeDelta = baseSize;
        }

        if(!myButtonPushed && !opponentButtonPushed)
        {
            SwitchTab(TabType.All);
        }
        else if(myButtonPushed)
        {
            SwitchTab(TabType.Myself);
        }
        else if(opponentButtonPushed)
        {
            SwitchTab(TabType.Opponent);
        }
    }

    private void SwitchTab(TabType tab)
    {
        currentTab = tab;
        
        switch(currentTab)
        {
            case TabType.All:
                myLogMenu.SetActive(false);
                oppLogMenu.SetActive(false);
                allLogMenu.SetActive(true);
                scrollRect.content = allLogMenu.GetComponent<RectTransform>();
                break;
            case TabType.Myself:
                myLogMenu.SetActive(true);
                oppLogMenu.SetActive(false);
                allLogMenu.SetActive(false);
                scrollRect.content = myLogMenu.GetComponent<RectTransform>();
                break;
            case TabType.Opponent:
                myLogMenu.SetActive(false);
                oppLogMenu.SetActive(true);
                allLogMenu.SetActive(false);
                scrollRect.content = oppLogMenu.GetComponent<RectTransform>();
                break;
        }
    }

    public void CloseDrawer()
    {
        StartCoroutine(SlideDrawer(-drawerPanel.rect.width));
    }

    // メニューを開く関数
    public void OpenDrawer()
    {
        StartCoroutine(SlideDrawer(-640));
    }

    // スライドアニメーションを行うコルーチン
    private IEnumerator SlideDrawer(float targetX)
    {
        float startX = drawerPanel.anchoredPosition.x;
        float elapsedTime = 0;

        while (elapsedTime < animationDuration)
        {
            float newX = Mathf.Lerp(startX, targetX, elapsedTime / animationDuration);
            drawerPanel.anchoredPosition = new Vector2(newX, drawerPanel.anchoredPosition.y);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        drawerPanel.anchoredPosition = new Vector2(targetX, drawerPanel.anchoredPosition.y);
    }
}