using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class guiScript : MonoBehaviour
{
    /// The script takes information from Player, displays it on the screen, and communicates to the Tile_Selector_Script whenever the user interacts with the button.
    public GameObject players;
    public GameObject tileSelector;

    public Player playerData;
    Tile_Selector_Script tileSelectorScript;

    void Start()
    {
        players = GameObject.FindGameObjectWithTag("Players");
        tileSelectorScript = tileSelector.GetComponent<Tile_Selector_Script>();
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 50, 100, 20), "Confirm moves"))
        {
            if (playerData != null && tileSelectorScript.pendingMoves > 0)
            {
                tileSelectorScript.confirm = true;
            }
        }

        if (GUI.Button(new Rect(10, 70, 100, 20), "Select Player 1"))
        {
            if (playerData != null)
            {
                tileSelectorScript.ClearPath();
                playerData.selected = false;
            }
            playerData = players.transform.GetChild(0).GetComponent<Player>();
            if (playerData.moves > 0)
            {
                tileSelectorScript.HighlightNeighbors(tileSelectorScript.tileMap.WorldToCell(playerData.transform.position));
            }
            tileSelectorScript.start = tileSelectorScript.tileMap.WorldToCell(playerData.transform.position);
            tileSelectorScript.playerCell = tileSelectorScript.start;
            tileSelectorScript.tileMap.SetTileFlags(tileSelectorScript.start, TileFlags.None);
            tileSelectorScript.tileMap.SetColor(tileSelectorScript.start, Color.magenta);
            playerData.selected = true;
        }

        if (GUI.Button(new Rect(10, 90, 100, 20), "Select Player 2"))
        {
            if (playerData != null)
            {
                tileSelectorScript.ClearPath();
                playerData.selected = false;
            }
            playerData = players.transform.GetChild(1).GetComponent<Player>();
            if (playerData.moves > 0)
            {
                tileSelectorScript.HighlightNeighbors(tileSelectorScript.tileMap.WorldToCell(playerData.transform.position));
            }
            tileSelectorScript.start = tileSelectorScript.tileMap.WorldToCell(playerData.transform.position);
            tileSelectorScript.playerCell = tileSelectorScript.start;
            tileSelectorScript.tileMap.SetTileFlags(tileSelectorScript.start, TileFlags.None);
            tileSelectorScript.tileMap.SetColor(tileSelectorScript.start, Color.magenta);
            playerData.selected = true;
        }

        if (playerData != null)
        {
            GUI.Label(new Rect(10, 10, 200, 20), "Moves remaining: " + playerData.moves);
            GUI.Label(new Rect(10, 30, 200, 20), "Moves pending: " + tileSelectorScript.pendingMoves);
        }
    }
}
