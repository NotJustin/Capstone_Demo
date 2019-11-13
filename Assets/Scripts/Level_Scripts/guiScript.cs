using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class guiScript : MonoBehaviour
{
    /// The script takes information from Player, displays it on the screen, and communicates to the Tile_Selector_Script whenever the user interacts with the button.
    public GameObject menu;

    Turn_Handler turnHandler;

    public int mode;

    void Start()
    {
        mode = 0;
        turnHandler = GetComponent<Turn_Handler>();
    }

    void OnGUI()
    {
        menu.transform.position = new Vector3(transform.position.x - 11.5f, transform.position.y + 4.0f, 0);
        if (!turnHandler.activePlayer.moving && GUI.Button(new Rect(5, (20 + 5), 100, 40), "Confirm moves"))
        {
            if (turnHandler.activePlayer != null && turnHandler.activePlayer.pendingMoves > 0)
            {
                turnHandler.confirm = true;
            }
        }

        /*if (!turnHandler.activePlayer.moving && GUI.Button(new Rect(5, (80 + 5), 100, 40), "Toggle Mode"))
        {
            mode = mode>0 ? 0:1;
        }*/

        if (!turnHandler.activePlayer.moving && GUI.Button(new Rect(5, (140 + 5), 100, 40), "End turn"))
        {
            if (turnHandler.playerTurn)
            {
                turnHandler.activePlayer.turnStarted = false;
                turnHandler.playerTurn = false;
                turnHandler.activePlayer.ClearPath();
                turnHandler.enemyTurn = true;
            }
        }

        GUI.Label(new Rect(5, 220, 200, 20), "Moves remaining: " + turnHandler.activePlayer.moves);
        GUI.Label(new Rect(5, 240, 200, 20), "Moves pending: " + turnHandler.activePlayer.pendingMoves);
        if (turnHandler.enemyTurn && turnHandler.activeEnemy != null)
        {
            GUI.Label(new Rect(5, 200, 200, 20), "Enemy room number: " + turnHandler.activeEnemy.tileRoom.number);
        }
        else if (turnHandler.activePlayer.tileRoom == null)
        {
            GUI.Label(new Rect(5, 200, 200, 20), "Player is in doorway");
        }
        else
        {
            GUI.Label(new Rect(5, 200, 200, 20), "Player room number: " + turnHandler.activePlayer.tileRoom.number);
        }
        if (turnHandler.enemyList.Count > 0 && turnHandler.activeEnemy.awaitMovement)
        {
            GUI.Label(new Rect(5, 180, 200, 20), "Selected: " + turnHandler.activeEnemy.obj.transform.name);
        }
        else
        {
            GUI.Label(new Rect(5, 180, 200, 20), "Selected: " + turnHandler.activePlayer.transform.name);
        }
    }
}
