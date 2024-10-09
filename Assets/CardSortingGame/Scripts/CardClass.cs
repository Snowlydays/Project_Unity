using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardClass : MonoBehaviour
{
    public GameObject cardObject;
    public int cardNumber;

    public CardClass(GameObject objCard, int num)
    {
        cardObject = objCard;
        cardNumber = num;
    }
}
