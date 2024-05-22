using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteBall : MonoBehaviour
{
    // Start is called before the first frame update
    public static bool isMoving = false;
    public static float Angle;
    float speed = 0.1f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(isMoving==true){
            Vector3 addTrans = new Vector3(
                speed*Mathf.Cos(Angle * Mathf.Deg2Rad),
                speed*Mathf.Sin(Angle * Mathf.Deg2Rad),
                0
            );
            this.gameObject.transform.position += addTrans;
        }
    }
}
