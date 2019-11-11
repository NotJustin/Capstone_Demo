using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class optionsButtonFX : MonoBehaviour
{
    public AudioSource myFX;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    public void HoverSound()
    {
        myFX.PlayOneShot(hoverSound);
    }
    public void ClickSound()
    {
        myFX.PlayOneShot(clickSound);
    }
}
