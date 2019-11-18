using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class card : MonoBehaviour
{
    string attackType;
    int cost;
    int range;
    int moves;
    int damageOne;
    int damageTwo;

    Transform canvas;
    Transform text;

    //public GameObject CardObject;
    void Start()
    {
        canvas = transform.Find("Canvas");
        text = canvas.Find("Text");
        attackType = "AoE";
        cost = 4;
    }
    void Update()
    {
        text.GetComponent<Text>().text = "Attack Type: " + attackType + " Energy: " + cost;
    }
}
