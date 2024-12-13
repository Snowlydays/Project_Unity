using System;
using UnityEngine;
using UnityEngine.UI;

public class CardNumManager : MonoBehaviour
{
    private const int InitCardNum = 5;
    private const int MinCardNum = 3;
    public static int MaxCardNum = 7;
    [SerializeField] private GameObject numberObj;
    
    [SerializeField] private Button increaseCardButton;
    [SerializeField] private Button decreaseCardButton;

    [SerializeField] private Sprite decDisabledSprite;
    [SerializeField] private Sprite decEnabledSprite;
    [SerializeField] private Sprite incDisabledSprite;
    [SerializeField] private Sprite incEnabledSprite;
    [SerializeField] private Sprite[] numberSprites = new Sprite[MaxCardNum];
    
    private void Awake()
    {
        ResetCardNum();
        increaseCardButton.onClick.AddListener(OnIncreaseCardButtonClicked);
        decreaseCardButton.onClick.AddListener(OnDecreaseCardButtonClicked);
    }
    
    private void OnIncreaseCardButtonClicked()
    {
        if (NetworkSystem.cardNum < MaxCardNum)
        {
            NetworkSystem.cardNum++;
            UpdateButtonUI();
        }
    }

    private void OnDecreaseCardButtonClicked()
    {
        if (NetworkSystem.cardNum > MinCardNum)
        {
            NetworkSystem.cardNum--;
            UpdateButtonUI();
        }
    }

    private void UpdateButtonUI()
    {
        Image numImage = numberObj.GetComponent<Image>();
        Image decImage = decreaseCardButton.GetComponent<Image>();
        Image incImage = increaseCardButton.GetComponent<Image>();
        numImage.sprite = numberSprites[NetworkSystem.cardNum - 1];
        decImage.sprite = (NetworkSystem.cardNum <= MinCardNum ? decDisabledSprite : decEnabledSprite);
        incImage.sprite = (NetworkSystem.cardNum >= MaxCardNum ? incDisabledSprite : incEnabledSprite);
    }

    public void ResetCardNum()
    {
        NetworkSystem.cardNum = InitCardNum;
        UpdateButtonUI();
    }
}