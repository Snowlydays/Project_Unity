using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    //効果音再生用オブジェクト
    //効果音を流し終わると自動的にオブジェクトが消滅する
    AudioSource audioSource;

    bool soundplayed=false;

    public void PlaySE(AudioClip se)
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(se);
        soundplayed=true;
    }

    void Update(){
        if(soundplayed==true){
            if(audioSource.isPlaying)Destroy(this);
        }
    }
}
