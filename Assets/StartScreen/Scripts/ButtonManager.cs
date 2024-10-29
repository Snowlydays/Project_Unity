using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public void ToBilliards()
    {
        SceneManager.LoadScene("Billiards");
    }

    public void ToCardSortingGame()
    {
        SceneManager.LoadScene("ChooseMatchSoA"); 
    }
}