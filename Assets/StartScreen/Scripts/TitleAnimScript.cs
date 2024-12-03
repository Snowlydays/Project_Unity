using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleAnimScript : MonoBehaviour
{
    // Start is called before the first frame update
    float middley;

    float time=0f;

    public GameObject soundManager;

    void Start()
    {
        middley=GetComponent<RectTransform>().anchoredPosition.y;

        if(GameObject.Find("SoundManager")==null){
            GameObject obj=Instantiate(soundManager);
            obj.name = soundManager.name;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GetComponent<RectTransform>().anchoredPosition=new Vector3(-10f,
        middley+50*Mathf.Sin(Mathf.PI*time),0f);
        time+=1f/100f;
    }
}
