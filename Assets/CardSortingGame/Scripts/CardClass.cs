using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardClass
{
    public GameObject cardObject;
    public int cardNum;

    public CardClass(GameObject objCard, int num)
    {
        cardObject = objCard;
        cardNum = num;
    }
}