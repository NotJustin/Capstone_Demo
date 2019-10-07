using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GUI_buttons : MonoBehaviour
{
    void OnGUI()
    {
        if (GUI.Button(new Rect(300, 200, 150, 40), "Start Game"))
        {
            SceneManager.LoadScene("Sample_Level", LoadSceneMode.Single);
        }
    }
}
