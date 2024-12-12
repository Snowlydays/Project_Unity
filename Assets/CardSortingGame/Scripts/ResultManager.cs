using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class ResultManager : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip drawSound;
    public AudioClip decideSound;
    public GameObject SoundObject;

    public Transform cardPanel = null;
    
    [SerializeField] public GameObject CardPrefab; // カードのプレハブ
    [SerializeField] public Sprite[] numberSprites = new Sprite[10]; // 数字のスプライト(0-9まで)
    
    void Start()
    {
        GameObject.Find("BackButton").GetComponent<Button>().onClick.AddListener(backtitle);
        AuthenticationService.Instance.SignOut();

        string scenename = SceneManager.GetActiveScene().name;

        Debug.Log(scenename);

        GameObject soundobj;

        if (scenename == "WinScene") cardPanel = GameObject.Find("WinCardPanel").transform;
        if (scenename == "LoseScene") cardPanel = GameObject.Find("LoseCardPanel").transform;
        if (scenename == "DrawScene") cardPanel = GameObject.Find("DrawCardPanel").transform;
        
        if(cardPanel == null)Debug.Log("cardpanel = null");
        AdjustPanel(cardPanel.GetComponent<RectTransform>(),85,10,10,10);
        for (int i = 0; i < NetworkSystem.cardNum; i++)
        {
            GameObject card = Instantiate(CardPrefab, cardPanel);
            Image cardsImage = card.GetComponent<Image>();
            cardsImage.sprite = numberSprites[ (NetworkManager.Singleton.IsHost ? NetworkSystem.hostCard[i] : NetworkSystem.clientCard[i])];
            card.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            DraggableCard draggable = card.GetComponent<DraggableCard>();
            draggable.isDraggable = false;
        }

        switch(scenename){
            case "WinScene":
            soundobj=Instantiate(SoundObject);
            soundobj.GetComponent<PlaySound>().PlaySE(winSound);
            break;
            case "LoseScene":
            soundobj=Instantiate(SoundObject);
            soundobj.GetComponent<PlaySound>().PlaySE(loseSound);
            break;
            case "DrawScene":
            soundobj=Instantiate(SoundObject);
            soundobj.GetComponent<PlaySound>().PlaySE(drawSound);
            break;
        }
    }

    void Update()
    {
        //networkmanagerが存在したら常に破棄
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }
    }

    void backtitle(){
        GameObject soundobj=Instantiate(SoundObject);
        soundobj.GetComponent<PlaySound>().PlaySE(decideSound);
        SceneManager.LoadScene("StartScene");
    }
    
    private void AdjustPanel(RectTransform panelRect, float cardWidth, float cardSpacing, float paddingLeft, float paddingRight)
    {
        float totalWidth = paddingLeft + paddingRight + (cardWidth * NetworkSystem.cardNum) + (cardSpacing * (NetworkSystem.cardNum - 1));
        panelRect.sizeDelta = new Vector2(totalWidth, panelRect.sizeDelta.y);
    }
}
