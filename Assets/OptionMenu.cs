using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionMenu : MonoBehaviour
{

    public AudioMixer audiomixer;


    public void SetVolume (Slider slider)
    {
        audiomixer.SetFloat("volume",slider.value);
        Debug.Log(slider.value);
    }
}
