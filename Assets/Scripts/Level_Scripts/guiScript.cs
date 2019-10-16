﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class guiScript : MonoBehaviour
{
    /// The script takes information from Player, displays it on the screen, and communicates to the Tile_Selector_Script whenever the user interacts with the button.
    public GameObject tileSelector;

    Turn_Handler turnHandler;
    Tile_Selector_Script tileSelectorScript;

    public int mode;

    void Start()
    {
        mode = 0;
        turnHandler = GetComponent<Turn_Handler>();
        tileSelectorScript = tileSelector.GetComponent<Tile_Selector_Script>();
        //tileSelectorScript.playerCell = tileSelectorScript.tileMap.WorldToCell(turnHandler.activePlayer.transform.position);
        //tileSelectorScript.start = tileSelectorScript.playerCell;
    }

    void OnGUI()
    {
        if (!turnHandler.activePlayer.moving && GUI.Button(new Rect(5, (20 + 5), 100, 40), "Confirm moves"))
        {
            if (turnHandler.activePlayer != null && turnHandler.activePlayer.pendingMoves > 0)
            {
                tileSelectorScript.confirm = true;
            }
        }

        if (!turnHandler.activePlayer.moving && GUI.Button(new Rect(5, (80 + 5), 100, 40), "Toggle Mode"))
        {
            mode = mode>0 ? 0:1;
        }

        if (!turnHandler.activePlayer.moving && GUI.Button(new Rect(5, (140 + 5), 100, 40), "End turn"))
        {
            turnHandler.changeTurn = true;
        }

        GUI.Label(new Rect(160, 70, 200, 20), "Moves remaining: " + turnHandler.activePlayer.moves);
        GUI.Label(new Rect(160, 90, 200, 20), "Moves pending: " + turnHandler.activePlayer.pendingMoves);
        GUI.Label(new Rect(160, 110, 200, 20), "Selected: " + turnHandler.activePlayer.transform.name);
    }
}
