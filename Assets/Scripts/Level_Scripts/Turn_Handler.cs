using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turn_Handler : MonoBehaviour
{
    List<Player> players;

    private int remainingPlayerTurns;
    private int enemyTurn;

    void Start()
    {
        players = new List<Player>();
        foreach(Transform child in transform)
        {
            players.Add(child.gameObject.GetComponent<Player>());
        }
        remainingPlayerTurns = players.Count;
        enemyTurn = 0;
    }

    void Update()
    {
        if (remainingPlayerTurns == 0)
        {
            enemyTurn = 1;
        }
    }
}