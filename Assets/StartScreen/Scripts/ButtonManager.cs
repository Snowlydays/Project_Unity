using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public AudioClip decideSound;
    public GameObject SoundObject;

    public void ToBilliards()
    {
        GameObject soundobj=Instantiate(SoundObject);
        soundobj.GetComponent<PlaySound>().PlaySE(decideSound);
        SceneManager.LoadScene("Billiards");
    }

    public void ToCardSortingGame()
    {
        GameObject soundobj=Instantiate(SoundObject);
        soundobj.GetComponent<PlaySound>().PlaySE(decideSound);
        SceneManager.LoadScene("ChooseMatchSoA"); 
    }
}