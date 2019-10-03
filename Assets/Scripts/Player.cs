using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    /// This script corrects the player's initial position to snap to the grid
    /// and stores the maxMoves/moves that the player has so that other scripts may view and change the numbers as fit.
    float zAxis = 10;
    public int maxMoves = 2;
    public int moves = 2;

    void Start()
    {
        transform.position = new Vector3(RoundOffset(transform.position.x), RoundOffset(transform.position.y), zAxis);
    }

    public float RoundOffset(float a)
    {
        int b = Mathf.RoundToInt(a);
        if (b > a)
        {
            return b - 0.5f;
        }
        else
        {
            return b + 0.5f;
        }

    }
}