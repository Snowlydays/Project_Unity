using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseManager : MonoBehaviour
{
    public AudioClip decideSound;
    public GameObject SoundObject;
    
    public void ToPrivateMatch()
    {
        GameObject soundobj=Instantiate(SoundObject);
        soundobj.GetComponent<PlaySound>().PlaySE(decideSound);
        SceneManager.LoadScene("ChooseServerSoA");
    }

    public void ToRandomMatch()
    {
        Debug.Log("ランダムマッチは未実装です");
    }

    public void ToBackTitle(){
        GameObject soundobj=Instantiate(SoundObject);
        soundobj.GetComponent<PlaySound>().PlaySE(decideSound);
        SceneManager.LoadScene("StartScene");
    }
}