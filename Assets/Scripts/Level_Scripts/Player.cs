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
    public float playerSpeed = 0.25f;
    public bool selected;
    public Animator animator;
    public Vector3 lastPosition;
    public bool started = false;

    /*struct PlayerStruct
    {
        GameObject player;
        int maxMoves;
        int moves;
        bool selected;
    }*/

    void Start()
    {
        animator = GetComponent<Animator>();
        lastPosition = new Vector3(-1, -1, 0);
    }

    private void FixedUpdate()
    {
        if (!started && lastPosition != transform.position)
        {
            started = true;
            transform.position = new Vector3(RoundOffset(transform.position.x), RoundOffset(transform.position.y), zAxis);
            lastPosition = transform.position;
            Debug.Log(transform.position);
        }
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