using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

/*
 * Card class, can be compared for sorting
 * and have a tooltip message...
 */ 
public class Card : MonoBehaviour,IComparable {
    
    public string tooltipMessage = "this is a tooltip message";

    [HideInInspector]
    public int id = 0;
 
    // Compare based on card id
    int IComparable.CompareTo(object obj)
    {
        Card otherCard = obj as Card;
        if(otherCard != null)
        {
            return id.CompareTo(otherCard.id);
        }
        else
        {
            return 0;
        }
        
    }
}
