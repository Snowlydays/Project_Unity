using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaptostartAnimScript : MonoBehaviour
{

    float time=0f;

    void FixedUpdate()
    {
        GetComponent<Image>().color=new Vector4(1f,
        1f,
        1f,
        0.5f+0.3f*Mathf.Sin(Mathf.PI*time));
        time+=1f/35f;
    }
}
