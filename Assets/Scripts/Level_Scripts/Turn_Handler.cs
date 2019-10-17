using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Turn_Handler : MonoBehaviour
{
    public List<Player> playerList;

    public GameObject players;

    private int remainingPlayerTurns;
    private int enemyTurn;
    public Player activePlayer;
    public bool changeTurn = false;

    public GameObject tileSelector;
    Tile_Selector_Script tileSelectorScript;

    void Start()
    {
        tileSelectorScript = tileSelector.GetComponent<Tile_Selector_Script>();
        playerList = new List<Player>();
        Vector3Int playerCell;
        foreach (Transform child in players.transform)
        {
            Player player = child.GetComponent<Player>();
            playerList.Add(player);
            player.start = tileSelectorScript.tileMap.WorldToCell(player.transform.position);
        }
        activePlayer = playerList[0];
        //playerList.RemoveAt(0);
        playerCell = tileSelectorScript.tileMap.WorldToCell(activePlayer.transform.position);
        tileSelectorScript.tileMap.SetTileFlags(playerCell, TileFlags.None);
        tileSelectorScript.tileMap.SetColor(playerCell, Color.magenta);

        /// Setting possible-to-select tiles as those surrounding the player, if the player has moves
        if (activePlayer.moves > 0)
        {
            tileSelectorScript.HighlightNeighbors(playerCell);
        }
    }

    void Update()
    {
        if (changeTurn)
        {
            /// Set changeTurn to false because we used it to get into this conditional and don't want to do it again.
            changeTurn = false;
            /// Unhighlights any yellow tiles
            tileSelectorScript.UnhighlightOldNeighbors();
            /// Unhighlights all selected tiles
            activePlayer.ClearPath();
            /// Add the current activePlayer to the end of the list
            playerList.Add(activePlayer);
            /// Remove the current activePlayer from the front of the list
            playerList.RemoveAt(0);
            /// Set activePlayer to be the new front of the list
            activePlayer = playerList[0];
            /// Get the coordinate of the activePlayer's cell on the tileMap
            Vector3Int playerCell = tileSelectorScript.tileMap.WorldToCell(activePlayer.transform.position);
            /// Set the flags to none so that we can change the color to magenta
            tileSelectorScript.tileMap.SetTileFlags(playerCell, TileFlags.None);
            /// Change the tile's color to magenta
            tileSelectorScript.tileMap.SetColor(playerCell, Color.magenta);
            /// Increment/reset things related to this character since their turn is just beginning now.
            activePlayer.StartTurn();
            /// Highlight the neighboring cells as yellow tiles.
            tileSelectorScript.HighlightNeighbors(playerCell);
        }
    }
}