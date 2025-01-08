using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandAnimScript : MonoBehaviour
{
    // Start is called before the first frame update
    float middlex,middley;

    float time=0f;

    void Start()
    {
        middlex=GetComponent<RectTransform>().anchoredPosition.x;
        middley=GetComponent<RectTransform>().anchoredPosition.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GetComponent<RectTransform>().anchoredPosition=new Vector3(
        middlex+10*Mathf.Sin(Mathf.PI*time),
        middley-10*Mathf.Sin(Mathf.PI*time),0f);
        time+=1f/30f;
    }

    public void SetPosition(float x, float y){
        GetComponent<RectTransform>().anchoredPosition = new Vector3(x,y,0f);
        middlex=GetComponent<RectTransform>().anchoredPosition.x;
        middley=GetComponent<RectTransform>().anchoredPosition.y;
    }
}
