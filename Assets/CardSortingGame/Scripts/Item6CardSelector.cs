using System;
using UnityEngine;
using UnityEngine.UI;

public class Item6CardSelector : MonoBehaviour
{
    // public int selectNumber = 5;
    private const int MinNumber = 1;
    [SerializeField] private GameObject numberObj;
    
    [SerializeField] private Button increaseNumberButton;
    [SerializeField] private Button decreaseNumberButton;

    [SerializeField] private Sprite decDisabledSprite;
    [SerializeField] private Sprite decEnabledSprite;
    [SerializeField] private Sprite incDisabledSprite;
    [SerializeField] private Sprite incEnabledSprite;
    [SerializeField] private Sprite[] numberSprites = new Sprite[CardNumManager.MaxCardNum];
    
    private void Awake()
    {
        increaseNumberButton.onClick.AddListener(OnIncreaseCardButtonClicked);
        decreaseNumberButton.onClick.AddListener(OnDecreaseCardButtonClicked);
    }
    
    private void OnIncreaseCardButtonClicked()
    {
        if (ItemUsingManager.chooseNumber < NetworkSystem.cardNum)
        {
            ItemUsingManager.chooseNumber++;
            UpdateButtonUI();
        }
    }

    private void OnDecreaseCardButtonClicked()
    {
        if (ItemUsingManager.chooseNumber > MinNumber)
        {
            ItemUsingManager.chooseNumber--;
            UpdateButtonUI();
        }
    }

    private void UpdateButtonUI()
    {
        Image numImage = numberObj.GetComponent<Image>();
        Image decImage = decreaseNumberButton.GetComponent<Image>();
        Image incImage = increaseNumberButton.GetComponent<Image>();
        numImage.sprite = numberSprites[ItemUsingManager.chooseNumber - 1];
        decImage.sprite = (ItemUsingManager.chooseNumber <= MinNumber ? decDisabledSprite : decEnabledSprite);
        incImage.sprite = (ItemUsingManager.chooseNumber >= NetworkSystem.cardNum ? incDisabledSprite : incEnabledSprite);
    }

    public void ResetItemSixSelectNumber()
    {
        int initNumber = NetworkSystem.cardNum / 2;
        ItemUsingManager.chooseNumber = initNumber;
        UpdateButtonUI();
    }
}