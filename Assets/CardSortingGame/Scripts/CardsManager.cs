using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsManager : MonoBehaviour
{
    public List<CardClass> myCards;
    // Start is called before the first frame update
    void Start()
    {
        MainSystemScript mainSystem = FindObjectOfType<MainSystemScript>();
        // Helpers helpers = FindObjectOfType<Helpers>();

        GameObject[] cardObjects = mainSystem.GetMyCards();
        int[] idx = GenRandomIdx(1, 7);
        for(int i = 0; i < cardObjects.Length; i++)
        {
            CardClass card = new CardClass(cardObjects[i], idx[i]);
            myCards.Add(card);
            Debug.Log(idx[i]);
        }
    }

    public int[] GenRandomIdx(int origin, int len)
    {
        int[] idx = new int[len];
        for(int i = 0; i < len; i++)
        {
            idx[i] = i + origin;
        }
        for(int i = 0; i < len; i++)
        {
            int j = Random.Range(0, len);
            (idx[i], idx[j]) = (idx[j], idx[i]);
        }
        return idx;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
