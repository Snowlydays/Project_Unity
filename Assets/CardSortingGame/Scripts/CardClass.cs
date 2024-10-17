using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardClass
{
    public GameObject cardObject;
    public int cardIdx;

    public CardClass(GameObject objCard, int num)
    {
        cardObject = objCard;
        cardIdx = num;
    }
}