using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class guiScript : MonoBehaviour
{
    /// The script takes information from Player, displays it on the screen, and communicates to the Tile_Selector_Script whenever the user interacts with the button.
    public GameObject player;
    public GameObject tileSelector;

    Player playerData;
    Tile_Selector_Script tileSelectorScript;

    void Start()
    {
        playerData = player.GetComponent<Player>();
        tileSelectorScript = tileSelector.GetComponent<Tile_Selector_Script>();
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 50, 100, 20), "Confirm moves"))
        {
            tileSelectorScript.confirm = true;
        }

        GUI.Label(new Rect(10, 10, 200, 20), "Moves remaining: " + playerData.moves);
        GUI.Label(new Rect(10, 30, 200, 20), "Moves pending: " + tileSelectorScript.pendingMoves);
    }
}
