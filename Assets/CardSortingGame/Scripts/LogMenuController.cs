using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LogMenuController : MonoBehaviour
{
    public Button allTabButton, myselfTabButton, opponentTabButton, closeButton;
    public TMP_Text contentText;

    public List<string> allLogs = new List<string>();
    public List<string> myLogs = new List<string>();
    public List<string> opponentLogs = new List<string>();

    public enum TabType { All, Myself, Opponent }
    private TabType currentTab = TabType.All;

    [SerializeField] private RectTransform drawerPanel;
    [SerializeField] private float animationDuration = 0.5f; // アニメーションの時間

    private NetworkSystem networkSystem;

    void Start()
    {
        allTabButton.onClick.AddListener(() => SwitchTab(TabType.All));
        myselfTabButton.onClick.AddListener(() => SwitchTab(TabType.Myself));
        opponentTabButton.onClick.AddListener(() => SwitchTab(TabType.Opponent));
        closeButton.onClick.AddListener(CloseDrawer);


        drawerPanel.anchoredPosition = new Vector2(-drawerPanel.rect.width, drawerPanel.anchoredPosition.y);

        networkSystem = FindObjectOfType<NetworkSystem>();
    }

    public void AddLogText(string str, TabType t)
    {
        if(t == TabType.All)allLogs.Add(str);
        else if(t == TabType.Myself)myLogs.Add(str);
        else if(t == TabType.Opponent)opponentLogs.Add(str);
    }

    private void SwitchTab(TabType tab)
    {
        currentTab = tab;
        
        switch(currentTab)
        {
            case TabType.All:
                DisplayLogEntries(allLogs);
                break;
            case TabType.Myself:
                DisplayLogEntries(myLogs);
                break;
            case TabType.Opponent:
                DisplayLogEntries(opponentLogs);
                break;
        }
    }

    private void DisplayLogEntries(List<string> entries)
    {
        contentText.text = string.Join("\n", entries);
    }
    
    public void CloseDrawer()
    {
        StartCoroutine(SlideDrawer(-drawerPanel.rect.width));
    }

    // メニューを開く関数
    public void OpenDrawer()
    {
        DisplayLogEntries(allLogs);
        StartCoroutine(SlideDrawer(0));
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
