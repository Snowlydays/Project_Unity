using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BubbleSort : MonoBehaviour
{
    // Start is called before the first frame update
    ballclass[] myball = new ballclass[5];
    void Start()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("ball");
        
        for(int i=0;i<5;i++){
            int ballnum = int.Parse(objects[i].name.Substring(5));
            myball[i] = new ballclass(objects[i],ballnum);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i=0;i<5;i++){
            Transform balltrans = myball[i].ballobject.transform;

            Vector3 pos = balltrans.position;

            pos.x=-0.8f*(3-i)+0.8f;

            balltrans.position = pos;
        }

        if(Input.anyKey){
            //myball.Sort ((a, b) => a.ballnumber - b.ballnumber);
            Array.Sort (myball, (a, b) => a.ballnumber - b.ballnumber);
        }
    }
}
