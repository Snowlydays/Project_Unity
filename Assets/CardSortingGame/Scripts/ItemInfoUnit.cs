using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemInfoUnit
{
    private ItemMenuController itemMenuController;
    public TabType tabType;
    public GameObject itemInfoObject;

    private ItemPhaseManager itemPhaseManager;
    
    public ItemInfoUnit(TabType tabType, int itemNum)
    {
        itemPhaseManager = Object.FindObjectOfType<ItemPhaseManager>();
        itemMenuController = Object.FindObjectOfType<ItemMenuController>();

        GameObject prefab = itemMenuController.itemInfoPrefab;
        Transform image = prefab.transform.Find("Image"); 
        image.Find("ItemIcon").GetComponent<Image>().sprite = itemPhaseManager.itemIcons[itemNum - 1];
        TextMeshProUGUI itemName = image.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI itemInfoText = image.transform.Find("ItemInfoText").GetComponent<TextMeshProUGUI>();
        itemName.text = ItemUsingManager.itemNameDict[itemNum];
        itemInfoText.text = ItemMenuController.itemDescriptionDict[itemNum];
        
        if(tabType == TabType.All)
        {
            this.itemInfoObject = Object.Instantiate(prefab, itemMenuController.allItemMenu.transform);
        }
        else if(tabType == TabType.Possesed)
        {
            this.itemInfoObject = Object.Instantiate(prefab, itemMenuController.possesedItemMenu.transform);
        }
    }
}