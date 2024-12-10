using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ItemMenuController : MonoBehaviour
{
    public Button allButton, closeButton;
    public List<LogUnit> allItems = new List<LogUnit>();
    public List<LogUnit> invItems = new List<LogUnit>();

    [SerializeField] public GameObject allItemMenu, possesedItemMenu;
    [SerializeField] public GameObject itemInfoPrefab;
    
    [SerializeField] private RectTransform drawerPanel;
    [SerializeField] private float animationDuration = 0.3f; // アニメーションの時間

    [SerializeField] private Sprite allButtonSprite;
    [SerializeField] private Sprite pushedAllButtonSprite;

    [SerializeField] private ScrollRect scrollRect;

    private TabType currentTab = TabType.Possesed;
    private RectTransform allButtonTransform;
    private Vector2 baseSize = new Vector2(646, 414);
    private Vector2 pushedSize = new Vector2(646, 375);
    private Image allButtonImage;

    public bool allButtonPushed = false;
    public bool activeButton = true;

    public ItemPhaseManager itemPhaseManager;
    private NetworkSystem networkSystem;

    void Start()
    {
        possesedItemMenu.SetActive(true);
        allItemMenu.SetActive(false);

        allButtonImage = allButton.GetComponent<Image>();
        allButtonTransform = allButton.GetComponent<RectTransform>();
        
        allButton.onClick.AddListener(() => ManageTabState());
        closeButton.onClick.AddListener(CloseDrawer);

        drawerPanel.anchoredPosition = new Vector2(drawerPanel.rect.width, drawerPanel.anchoredPosition.y);
        networkSystem = FindObjectOfType<NetworkSystem>();
    }

    private void ManageTabState()
    {
        allButtonPushed = !allButtonPushed;

        if(allButtonPushed)
        {
            allButtonImage.sprite = pushedAllButtonSprite;
            allButtonTransform.sizeDelta = pushedSize;
        }
        else
        {
            allButtonImage.sprite = allButtonSprite;
            allButtonTransform.sizeDelta = baseSize;
        }

        if(allButtonPushed)
        {
            SwitchTab(TabType.All);
        }
        else
        {
            SwitchTab(TabType.Possesed);
        }
    }

    private void SwitchTab(TabType tab)
    {
        currentTab = tab;
        
        switch(currentTab)
        {
            case TabType.Possesed:
                allItemMenu.SetActive(false);
                possesedItemMenu.SetActive(true);
                scrollRect.content = allItemMenu.GetComponent<RectTransform>();
                break;
            case TabType.All:
                allItemMenu.SetActive(true);
                possesedItemMenu.SetActive(false);
                scrollRect.content = possesedItemMenu.GetComponent<RectTransform>();
                break;
        }
    }

    public void CloseDrawer()
    {
        StartCoroutine(SlideDrawer(drawerPanel.rect.width));
    }

    // メニューを開く関数
    public void OpenDrawer()
    {
        StartCoroutine(SlideDrawer(64));
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
