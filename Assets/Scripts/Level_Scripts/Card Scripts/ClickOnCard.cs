using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickOnCard : MonoBehaviour
{
    public GameObject guiObject;
    public guiScript gui;
    public GenericCard card;
    void Start()
    {
        gui = guiObject.GetComponent<guiScript>();
    }
    void OnMouseDown()
    {
        gui.selectedCard = card;
    }
}
