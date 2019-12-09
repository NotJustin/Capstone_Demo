using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMainButton : MonoBehaviour
{
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 40), "Back to Main Menu"))
        {
            SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
        }
    }
}
