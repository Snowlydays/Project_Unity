using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseManager : MonoBehaviour
{
    public void ToPrivateMatch()
    {
        SceneManager.LoadScene("ChooseServerSoA");
    }

    public void ToRandomMatch()
    {
        Debug.Log("ランダムマッチは未実装です");
    }
}