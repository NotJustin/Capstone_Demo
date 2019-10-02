using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class guiScript : MonoBehaviour
{
    public Texture btnTexture;

    public GameObject player;
    Player playerData;

    public GameObject tileSelector;
    Tile_Selector_Script tileSelectorScript;

    void Start()
    {
        playerData = player.GetComponent<Player>();
        tileSelectorScript = tileSelector.GetComponent<Tile_Selector_Script>();
    }

    void OnGUI()
    {
        if (!btnTexture)
        {
            Debug.LogError("Please assign a texture on the inspector");
            return;
        }

        if (GUI.Button(new Rect(10, 50, 100, 20), "Confirm moves"))
            tileSelectorScript.confirm = true;

        GUI.Label(new Rect(10, 10, 200, 20), "Moves remaining: " + playerData.moves);
        GUI.Label(new Rect(10, 30, 200, 20), "Moves pending: " + tileSelectorScript.pendingMoves);
    }
}
